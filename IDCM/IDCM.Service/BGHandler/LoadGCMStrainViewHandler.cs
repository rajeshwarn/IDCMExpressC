﻿using IDCM.Data.Base;
using IDCM.Service.Common;
using IDCM.Service.UIM;
using IDCM.Service.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

/********************************
* Individual Data Center of Microbial resources (IDCM)
* A desktop software package for microbial resources researchers.
* 
* Licensed under the Apache License, Version 1.0. See License.txt in the project root for license information.
* 
* @Contact NO.1 Beichen West Road, Chaoyang District, Beijing 100101, Email: office@im.ac.cn
*/
namespace IDCM.Service.BGHandler
{
    /// <summary>
    /// 本类的主要功能是异步执行GCM模块中节点树的加载
    /// </summary>
    public class LoadGCMStrainViewHandler:AbsHandler
    {
        public LoadGCMStrainViewHandler(GCMSiteMHub gcmSite,string strainid, TreeView treeView_record, ListView listView_record)
        {
            // TODO: Complete member initialization
            this.strainid = strainid;
            this.treeView_record = treeView_record;
            this.listView_record = listView_record;
            this.gcmSite = gcmSite;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="cancel"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object doWork(System.ComponentModel.BackgroundWorker worker, bool cancel, List<object> args)
        {
            TreeView treeView = LoadGCMRecordNode.loadData(gcmSite, strainid, listView_record);
            if (treeView == null) return new Object();
            TreeViewAsyncUtil.syncClearNodes(treeView_record);
            TreeViewAsyncUtil.syncAddNodes(treeView_record, treeView);
            return new Object();
        }

        private GCMSiteMHub gcmSite = null;
        private String strainid;
        private TreeView treeView_record;
        private ListView listView_record;
    }
}