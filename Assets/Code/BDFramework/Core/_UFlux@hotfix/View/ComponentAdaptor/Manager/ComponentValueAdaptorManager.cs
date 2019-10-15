﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using BDFramework.Mgr;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BDFramework.UFlux
{
    public class ComponentValueAdaptorManager : ManagerBase<ComponentValueAdaptorManager, ComponentAdaptorProcessAttribute>
    {
        Dictionary<Type, AComponentAdaptor> adaptorMap = new Dictionary<Type, AComponentAdaptor>();

        public override void Start()
        {
            base.Start();
            foreach (var cd in this.GetAllClassDatas())
            {
                var attr = cd.Attribute as ComponentAdaptorProcessAttribute;
                adaptorMap[attr.ComponentType] = CreateInstance<AComponentAdaptor>(cd);
            }

            //执行30s清理一次的cache
            IEnumeratorTool.StartCoroutine(this.IE_ClearCache(30));
        }


        /// <summary>
        /// 每个component的值
        /// </summary>
        public class ComponentValueCahce
        {
            public UIBehaviour UIBehaviour;
            public Transform Transform;
            public ComponentValueBind ValueBind;
            public object LastValue;
        }

        //
        Dictionary<int, Dictionary<string, ComponentValueCahce>> componentValueCacheMap = new Dictionary<int, Dictionary<string, ComponentValueCahce>>();

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aState"></param>
        public void SetComponentValue(Transform t, AStateBase aState)
        {
            //第一次进行缓存绑定后，就不再重新解析了，
            //所以使用者要保证每次的 state尽量是一致的
            Dictionary<string, ComponentValueCahce> map = null;
            var key = t.GetInstanceID();
            if (!componentValueCacheMap.TryGetValue(key, out map))
            {
                map = TransformStateBind(t, aState);
                componentValueCacheMap[key] = map;
            }
            while (true)
            {
                var field = aState.GetPropertyChange();
                if (field == null) break;
                //开始
                ComponentValueCahce cvc = null;
                if (map.TryGetValue(field, out cvc))
                {
                    //考虑 是否二次验证值是否改变
                    var newValue = aState.GetValue(field);
                    //执行赋值操作
                    if (cvc.UIBehaviour) //UI操作
                    {
                        adaptorMap[cvc.ValueBind.Type].SetData(cvc.UIBehaviour,cvc.ValueBind.FieldName,newValue);
                    }
                    else if (cvc.Transform) //自定义逻辑管理
                    {
                        adaptorMap[cvc.ValueBind.Type].SetData(cvc.Transform,cvc.ValueBind.FieldName,newValue);
                    }
                    cvc.LastValue = newValue;
                }
                else
                {
                    BDebug.LogError("State不存在字段:" + field);
                }
            }
        }


        /// <summary>
        /// bind Tansform和State的值，防止每次都修改
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aState"></param>
        /// <returns></returns>
        private Dictionary<string, ComponentValueCahce> TransformStateBind(Transform t, AStateBase aState)
        {
            Dictionary<string, ComponentValueCahce> retMap = new Dictionary<string, ComponentValueCahce>();
            //所有的成员信息
            var map = StateFactory.GetCache(aState.GetType());
            foreach (var mi in map.Values)
            {
                //先寻找节点 
                Transform transform = null;
                {
                    var attrs = mi.GetCustomAttributes(typeof(TransformPath), false);
                    if (attrs.Length > 0)
                    {
                        var attr = attrs[0] as TransformPath;
                        transform = t.Find(attr.Path);
                        if (!transform)
                        {
                            BDebug.LogError("节点不存在:" + attr.Path + "  -" + aState.GetType().Name);
                            continue;
                        }
                    }
                    else
                        continue;
                }
                //再进行值绑定
                {
                    var attrs = mi.GetCustomAttributes(typeof(ComponentValueBind), false);
                    if (attrs.Length > 0)
                    {
                        var attr = attrs[0] as ComponentValueBind;
                        if (attr != null)
                        {
                            ComponentValueCahce cvc = new ComponentValueCahce();
                            cvc.UIBehaviour = transform.GetComponent(attr.Type) as UIBehaviour;
                            if (!cvc.UIBehaviour)
                            {
                                cvc.Transform = transform;
                            }
                            cvc.ValueBind = attr;
                            //缓存
                            retMap[mi.Name] = cvc;
                        }
                    }
                }
            }

            return retMap;
        }



        /// <summary>
        /// 清理cache
        /// </summary>
        /// <returns></returns>
        private IEnumerator IE_ClearCache(float time)
        {
            while (true)
            {
                var keys = componentValueCacheMap.Keys.ToList();
                foreach (var key in keys)
                {
                    var cvcMap = componentValueCacheMap[key];
                    //遍历cache
                    var cvcMapkeys = cvcMap.Keys.ToList();
                    foreach (var ckey in cvcMapkeys)
                    {
                        var cvc = cvcMap[ckey];
                        if (!cvc.UIBehaviour)
                        {
                            cvcMap.Remove(ckey);
                        }
                    }
                    if (cvcMap.Count == 0)
                    {
                        componentValueCacheMap.Remove(key);
                    }

                }
                //每n s 清理一次
                yield return new WaitForSeconds(time);
            }
        }
    }
}