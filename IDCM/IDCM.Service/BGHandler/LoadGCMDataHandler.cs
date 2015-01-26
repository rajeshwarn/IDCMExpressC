﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using IDCM.Service.BGHandler;
using IDCM.Service.UIM;
using IDCM.Service.Common;
using IDCM.Data.Base;
using IDCM.Service.Utils;

namespace IDCM.Service.BGHandler
{

    public class LoadGCMDataHandler : AbsHandler
    {
        public LoadGCMDataHandler(GCMSiteMHub gcmSite,DataGridView itemDGV,TreeView recordTree,ListView recordView,Dictionary<string, int> loadedNoter)
        {
            this.itemDGV = itemDGV;
            this.recordTree=recordTree;
            this.recordView = recordView;
            this.loadedNoter = loadedNoter;
            this.gcmSite = gcmSite;
        }
        /// <summary>
        /// 后台任务执行方法的主体部分，异步执行代码段！
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="args"></param>
        public override Object doWork(BackgroundWorker worker, bool cancel, List<Object> args)
        {
            bool res = false;
            loadedNoter.Clear();
            res = GCMItemsLoader.loadData(gcmSite, itemDGV, loadedNoter, recordTree, recordView);
            return new object[] { res };
        }
       
        private GCMSiteMHub gcmSite = null;
        private DataGridView itemDGV = null;
        private TreeView recordTree=null;
        private ListView recordView=null;
        private Dictionary<string, int> loadedNoter = null;

    }
}
