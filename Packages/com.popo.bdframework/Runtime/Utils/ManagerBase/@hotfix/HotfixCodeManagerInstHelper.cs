﻿using System;
using System.Collections.Generic;
using System.Reflection;
using BDFramework.Hotfix.Reflection;
using BDFramework.Mgr;
using UnityEngine;

namespace BDFramework.HotFix.Mgr
{
    /// <summary>
    /// 热更管理器单例工具
    /// </summary>
    static public class HotfixCodeManagerInstHelper
    {
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        static public List<IMgr> LoadManagerInstance(IEnumerable<Type> types)
        {
            //管理器列表
            BDebug.Log("11111");
            var mgrList = new List<IMgr>();
            foreach (var type in types)
            {
                if (type != null && type.BaseType != null && type.BaseType.FullName != null)
                {
                    if (type.BaseType.FullName.Contains(".ManagerBase`2")) //这里ILR里面只能这么做，丑但有效
                    {
                        BDebug.Log("[hotfix]加载管理器-" + type.FullName, Color.green);
                        var inst = type.BaseType.GetProperty("Inst").GetValue(null, null);
                        var mgr = inst as IMgr;
                        if (mgr != null)
                        {
                            mgrList.Add(mgr);
                        }
                        else
                        {
                            BDebug.LogError("[hotfix]加载管理器失败-" + type.FullName);
                        }
                    }
                }
            }
          
            
            //按执行顺序排序
            mgrList.Sort((a, b) =>
            {
                var aAttr = a.GetType().GetCustomAttribute<ManagerOrder>();
                var bAttr = a.GetType().GetCustomAttribute<ManagerOrder>();
                var aOrder = aAttr == null ? 0 : aAttr.Order;
                var bOrder = bAttr == null ? 0 : bAttr.Order;
                //对比
                return aOrder.CompareTo(bOrder);
            });
            

            foreach (var type in types)
            {
                if (type != null && type.IsClass)
                {
                    var mgrAttributes = type.GetAttributeInILRuntimes<ManagerAttribute>();
                    if (mgrAttributes != null)
                    {
                        //注册类型
                        foreach (var mgr in mgrList)
                        {
                           
                            var ret = mgr.CheckType(type, mgrAttributes);
                            if (ret)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            BDebug.Log("管理器CheckType完成", Color.green);
            
            BDebug.Log("管理器数量"+mgrList.Count, Color.red);
            //管理器初始化
            foreach (var m in mgrList)
            {
                BDebug.Log("[hotfix]init"+m.GetType().FullName, Color.yellow);
                m.Init();
            }
            BDebug.Log("[hotfix]管理器Init完成", Color.green);
            return mgrList;
        }
    }
}