﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IDCM.Data.Base;
using IDCM.Data;
using System.Data;

namespace IDCM.Service.Common.DAM
{
    class CustomTColDefDAM
    {
        /// <summary>
        /// 检查数据表配置存在与否，如不存在则创建默认表属性设定
        /// </summary>
        public static bool checkTableSetting(WorkSpaceManager wsm)
        {
            string cmd = "SELECT * FROM " + typeof(CustomTColDef).Name;
            List<CustomTColDef> ctcds = DataSupporter.SQLQuery<CustomTColDef>(wsm, cmd);
            if (ctcds != null && ctcds.Count > 0)
            {
                ColumnMappingHolder.queryCacheAttrDBMap();
                return true;
            }
            else
            {
                CTableSetting.buildDefaultSetting();
                DataTable table = DataSupporter.SQLQuery(wsm, cmd);
                if (table != null && table.Rows.Count > 0)
                {
                    CustomTColMapDAM.buildCustomTable();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 保存一条新实例数据
        /// </summary>
        /// <param name="ctcd"></param>
        /// <returns></returns>
        public static int save(WorkSpaceManager wsm,params CustomTColDef[] ctcds)
        {
            List<string> cmds = new List<string>();
            foreach (CustomTColDef ctcd in ctcds)
            {
                StringBuilder cmdBuilder = new StringBuilder();
                cmdBuilder.Append("insert Or Replace into " + typeof(CustomTColDef).Name
                    + "(attr,attrType,comments,defaultVal,corder,isInter,isRequire,isUnique,restrict) values(");
                cmdBuilder.Append("'").Append(ctcd.Attr).Append("',");
                if (ctcd.AttrType == null)
                    cmdBuilder.Append("null,");
                else
                    cmdBuilder.Append("'").Append(ctcd.AttrType).Append("',");
                if (ctcd.Comments == null)
                    cmdBuilder.Append("null,");
                else
                    cmdBuilder.Append("'").Append(ctcd.Comments).Append("',");
                if (ctcd.DefaultVal == null)
                    cmdBuilder.Append("null,");
                else
                    cmdBuilder.Append("'").Append(ctcd.DefaultVal).Append("',");
                cmdBuilder.Append(ctcd.Corder).Append(",");
                cmdBuilder.Append("'").Append(ctcd.IsInter).Append("',");
                cmdBuilder.Append("'").Append(ctcd.IsRequire).Append("',");
                cmdBuilder.Append("'").Append(ctcd.IsUnique).Append("',");
                if (ctcd.Restrict == null)
                    cmdBuilder.Append("null");
                else
                    cmdBuilder.Append("'").Append(ctcd.Restrict).Append("'");
                cmdBuilder.Append(");");
                cmds.Add(cmdBuilder.ToString());
            }
            if (cmds.Count > 0)
                return SQLiteHelper.ExecuteNonQuery(ConnectStr, cmds.ToArray());
            return -1;
        }
        /// <summary>
        /// clear all setting
        /// </summary>
        public static void clearAll()
        {
            string cmd = "delete FROM CustomTColDef";
            SQLiteHelper.ExecuteNonQuery(ConnectStr, cmd);
        }
        /// <summary>
        /// 查询所有数据表属性定义对象
        /// </summary>
        /// <returns></returns>
        public static List<CustomTColDef> loadAll(WorkSpaceManager wsm,bool refresh = true)
        {
            if (refresh)
            {
                lock (ctcdCache)
                {
                    ctcdCache.Clear();
                    string cmd = "SELECT * FROM CustomTColDef order by corder";
                    DataTable table = SQLiteHelper.ExecuteDataTable(ConnectStr, cmd);
                    foreach (DataRow dr in table.Rows)
                    {
                        CustomTColDef ctcd = new CustomTColDef();
                        ctcd.Attr = dr["attr"].ToString();
                        ctcd.AttrType = dr["attrtype"].ToString();
                        ctcd.Comments = dr["comments"].ToString();
                        ctcd.DefaultVal = dr.IsNull("defaultVal") ? null : dr["defaultVal"].ToString();
                        ctcd.Corder = Convert.ToInt32(dr["corder"]);
                        ctcd.IsInter = Convert.ToBoolean(dr["isInter"].ToString());
                        ctcd.IsRequire = Convert.ToBoolean(dr["isRequire"].ToString());
                        ctcd.IsUnique = Convert.ToBoolean(dr["isUnique"].ToString());
                        ctcd.Restrict = dr["restrict"].ToString();
                        ctcdCache.Add(ctcd.Attr, ctcd);
                    }
                }
            }
            return ctcdCache.Values.ToList<CustomTColDef>();
        }
        /// <summary>
        /// 查询所有数据表自定义属性记录
        /// </summary>
        /// <returns></returns>
        public static LinkedList<CustomTColDef> loadCustomAll(WorkSpaceManager wsm)
        {
            LinkedList<CustomTColDef> customList = new LinkedList<CustomTColDef>();
            string cmd = "SELECT * FROM CustomTColDef where isInter='" + false.ToString() + "' order by corder ";
            DataTable table = SQLiteHelper.ExecuteDataTable(ConnectStr, cmd);
            foreach (DataRow dr in table.Rows)
            {
                CustomTColDef ctcd = new CustomTColDef();
                ctcd.Attr = dr["attr"].ToString();
                ctcd.AttrType = dr["attrtype"].ToString();
                ctcd.Comments = dr["comments"].ToString();
                ctcd.DefaultVal = dr.IsNull("defaultVal") ? null : dr["defaultVal"].ToString();
                ctcd.Corder = Convert.ToInt32(dr["corder"]);
                ctcd.IsInter = Convert.ToBoolean(dr["isInter"].ToString());
                ctcd.IsRequire = Convert.ToBoolean(dr["isRequire"].ToString());
                ctcd.IsUnique = Convert.ToBoolean(dr["isUnique"].ToString());
                ctcd.Restrict = dr["restrict"].ToString();
                customList.AddLast(ctcd);
            }
            return customList;
        }
        public static void appendCustomTColDef(CustomTColDef ctcd,WorkSpaceManager wsm)
        {
            //alter table
            CustomTColMapDAM.alterCustomTable_add(ctcd,wsm);
            //add ctcd
            CustomTColDefDAM.save(ctcd,wsm);
            //add to ctcdcache
            ctcdCache.Add(ctcd.Attr, ctcd);
        }
        public static void updateCustomTColDef(CustomTColDef ctcd,WorkSpaceManager wsm)
        {
            CustomTColDef pctcd = ctcdCache[ctcd.Attr];
            if (!pctcd.Corder.Equals(ctcd.Corder))
            {
                int viewOrder = ctcd.IsRequire ? ctcd.Corder : (ColumnMappingHolder.MaxMainViewCount + ctcd.Corder);
                ColumnMappingHolder.noteDefaultColMap(ctcd.Attr, ctcd.Corder, viewOrder);
            }
            //update ctcd
            save(ctcd,wsm);
            //update ctcdcache
            ctcdCache[ctcd.Attr] = ctcd;
        }
        /// <summary>
        /// 获取用户自定义表字段声明
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static CustomTColDef getCustomTColDef(string attr,WorkSpaceManager wsm)
        {
            if (ctcdCache.Count < 1)
                loadAll(wsm);//SELECT * FROM CustomTColDef order by corder
            CustomTColDef ctcd = null;
            ctcdCache.TryGetValue(attr, out ctcd);
            return ctcd;
        }

        /// <summary>
        /// 用户自定义表字段声明缓冲池
        /// </summary>
        protected static Dictionary<string, CustomTColDef> ctcdCache = new Dictionary<string, CustomTColDef>();

    }
}
