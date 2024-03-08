﻿
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using System.Collections.Generic;

namespace MyDotnet.Helper
{
    /// <summary>
    /// 泛型递归求树形结构
    /// </summary>
    public static class RecursionHelper
    {
        public static void LoopToAppendChildren(List<PermissionTree> all, PermissionTree curItem, long pid, bool needbtn)
        {
            var subItems = all.Where(ee => ee.Pid == curItem.value).ToList();

            var btnItems = subItems.Where(ss => ss.isbtn == true).ToList();
            if (subItems.Count > 0)
            {
                curItem.btns = new List<PermissionTree>();
                curItem.btns.AddRange(btnItems);
            }
            else
            {
                curItem.btns = new List<PermissionTree>();
            }

            if (!needbtn)
            {
                subItems = subItems.Where(ss => ss.isbtn == false).ToList();
            }

            if (subItems.Count > 0)
            {
                curItem.children = new List<PermissionTree>();
                curItem.children.AddRange(subItems);
            }
            else
            {
                curItem.children = new List<PermissionTree>();
            }

            if (curItem.isbtn)
            {
                //curItem.label += "按钮";
            }

            foreach (var subItem in subItems)
            {
                if (subItem.value == pid && pid > 0)
                {
                    //subItem.disabled = true;//禁用当前节点
                }

                LoopToAppendChildren(all, subItem, pid, needbtn);
            }
        }
        public static void LoopToAppendChildren(List<DepartmentTree> all, DepartmentTree curItem, long pid)
        {
            var subItems = all.Where(ee => ee.Pid == curItem.value).ToList();

            if (subItems.Count > 0)
            {
                curItem.children = new List<DepartmentTree>();
                curItem.children.AddRange(subItems);
            }
            else
            {
                curItem.children = new List<DepartmentTree>();
            }

            foreach (var subItem in subItems)
            {
                if (subItem.value == pid && pid > 0)
                {
                    //subItem.disabled = true;//禁用当前节点
                }

                LoopToAppendChildren(all, subItem, pid);
            }
        }
        /// <summary>
        /// 菜单列表
        /// </summary>
        /// <param name="all"></param>
        /// <param name="curItem"></param>
        /// <param name="pid"></param>
        /// <param name="allApi"></param>
        public static void LoopToAppendChildren(List<Permission> all, Permission curItem, long pid, List<Modules> allApi)
        {
            var subItems = all.Where(ee => ee.Pid == curItem.Id).ToList();
            curItem.MName = allApi.FirstOrDefault(d => d.Id == curItem.Mid)?.LinkUrl;
            if (subItems.Count > 0)
            {

                foreach (var subItem in subItems)
                {
                    subItem.MName = allApi.FirstOrDefault(d => d.Id == subItem.Mid)?.LinkUrl;
                }

                curItem.children = subItems;
            }
            else
            {
                curItem.children = new List<Permission>();
            }

            foreach (var subItem in subItems)
            {
                if (subItem.Id == pid && pid > 0)
                {
                    //subItem.disabled = true;//禁用当前节点
                }

                LoopToAppendChildren(all, subItem, pid, allApi);
            }
        }


        public static void LoopNaviBarAppendChildren(List<NavigationBar> all, NavigationBar curItem)
        {
            var subItems = all.Where(ee => ee.pid == curItem.id).ToList();

            if (subItems.Count > 0)
            {
                curItem.children = new List<NavigationBar>();
                curItem.children.AddRange(subItems);
            }
            else
            {
                curItem.children = new List<NavigationBar>();
            }


            foreach (var subItem in subItems)
            {
                LoopNaviBarAppendChildren(all, subItem);
            }
        }


        public static void LoopToAppendChildrenT<T>(List<T> all, T curItem, string parentIdName = "Pid", string idName = "value", string childrenName = "children")
        {
            var subItems = all.Where(ee => ee.GetType().GetProperty(parentIdName).GetValue(ee, null).ToString() == curItem.GetType().GetProperty(idName).GetValue(curItem, null).ToString()).ToList();

            if (subItems.Count > 0) curItem.GetType().GetField(childrenName).SetValue(curItem, subItems);
            foreach (var subItem in subItems)
            {
                LoopToAppendChildrenT(all, subItem);
            }
        }

        /// <summary>
        /// 将父子级数据结构转换为普通list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> TreeToList<T>(List<T> list, Action<T, T, List<T>> action = null)
        {
            List<T> results = new List<T>();
            foreach (var item in list)
            {
                results.Add(item);
                OperationChildData(results, item, action);
            }

            return results;
        }

        /// <summary>
        /// 递归子级数据
        /// </summary>
        /// <param name="allList">树形列表数据</param>
        /// <param name="item">Item</param>
        public static void OperationChildData<T>(List<T> allList, T item, Action<T, T, List<T>> action)
        {
            dynamic dynItem = item;
            if (dynItem.Children == null) return;
            if (dynItem.Children.Count <= 0) return;
            allList.AddRange(dynItem.Children);
            foreach (var subItem in dynItem.Children)
            {
                action?.Invoke(item, subItem, allList);
                OperationChildData(allList, subItem, action);
            }
        }
    }
}