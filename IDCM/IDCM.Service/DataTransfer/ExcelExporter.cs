﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.Data;
using System.IO;
using IDCM.Data.Base;
using IDCM.Data.Base.Utils;
using IDCM.Data;
using IDCM.Service.Common;

namespace IDCM.Service.DataTransfer
{
    class ExcelExporter
    {
        /// <summary>
        /// 导出数据到excel，数据源从数据库读取
        /// </summary>
        /// <param name="datasource">DataSourceMHub句柄，主要封装WorkSpaceManager</param>
        /// <param name="filepath">导出路径</param>
        /// <param name="cmdstr">查询字符串</param>
        /// <param name="tcount">总记录数</param>
        /// <returns></returns>
        public bool exportExcel(DataSourceMHub datasource, string filepath, string cmdstr, int tcount)
        {
            try
            {
                IWorkbook workbook = null;
                string suffix = Path.GetExtension(filepath).ToLower();
                if (suffix.Equals(".xlsx"))
                    workbook = new XSSFWorkbook();
                else
                    workbook = new HSSFWorkbook();
                ISheet sheet= workbook.CreateSheet("Core Datasets");
                IRow rowHead = sheet.CreateRow(0);
                HashSet<int> excludes = new HashSet<int>();
                //填写表头
                Dictionary<string, int> maps = LocalRecordMHub.getCustomAttrDBMapping(datasource);
                //填写表头
                int i = 0;
                foreach (string key in maps.Keys)
                {
                    ICell cell = rowHead.CreateCell(i++, CellType.String);
                    cell.SetCellValue(key);
                    cell.CellStyle.FillBackgroundColor = IndexedColors.Green.Index;
                }
                CellRangeAddress cra = CellRangeAddress.ValueOf("A1:" + numToExcelIndex(maps.Count)+"1");
                sheet.SetAutoFilter(cra);
                //填写内容
                int ridx = 1;
                int offset = 0;
                int stepLen =SysConstants.EXPORT_PAGING_COUNT;
                while (offset < tcount)
                {
                    int lcount = tcount - offset > stepLen ? stepLen : tcount - offset;
                    DataTable table = LocalRecordMHub.queryCTDRecordByHistSQL(datasource, cmdstr, lcount, offset);
                    foreach (DataRow row in table.Rows)
                    {
                        IRow srow = sheet.CreateRow(ridx++);
                        mergeDataToSheetRow(maps, row, srow);
                    }
                    offset += lcount;
                }
                using (FileStream fs = File.Create(filepath))
                {
                    workbook.Write(fs);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR::" + ex.Message + "\n" + ex.StackTrace);
                log.Error(ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 导出数据到excel，数据源DataGridViewSelectedRowCollection
        /// </summary>
        /// <param name="datasource">DataSourceMHub句柄，主要封装WorkSpaceManager</param>
        /// <param name="filepath">导出路径</param>
        /// <param name="selectedRows">数据源</param>
        /// <returns></returns>
        public bool exportExcel(DataSourceMHub datasource, string filepath, DataGridViewSelectedRowCollection selectedRows)
        {
            try
            {
                IWorkbook workbook = null;
                string suffix = Path.GetExtension(filepath).ToLower();
                if (suffix.Equals(".xlsx"))
                    workbook = new XSSFWorkbook();
                else
                    workbook = new HSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Core Datasets");
                IRow rowHead = sheet.CreateRow(0);
                HashSet<int> excludes = new HashSet<int>();
                //填写表头
                Dictionary<string, int> maps = LocalRecordMHub.getCustomAttrDBMapping(datasource);
                //填写表头
                int i = 0;
                foreach (string key in maps.Keys)
                {
                    ICell cell = rowHead.CreateCell(i++, CellType.String);
                    cell.SetCellValue(key);
                    cell.CellStyle.FillBackgroundColor = IndexedColors.Green.Index; 
                }
                CellRangeAddress cra = CellRangeAddress.ValueOf("A1:" + numToExcelIndex(maps.Count) + "1");
                sheet.SetAutoFilter(cra);
                //填写内容
                int IrowIndex = 1;
                for (int ridx = selectedRows.Count -1; ridx >=0; ridx--)
                {
                    DataGridViewRow dgvRow = selectedRows[ridx];
                    string recordId = dgvRow.Cells[CTDRecordA.CTD_RID].Value as string;
                    DataTable table = LocalRecordMHub.queryCTDRecord(datasource, null, recordId);
                    foreach (DataRow row in table.Rows)
                    {
                        IRow srow = sheet.CreateRow(IrowIndex++);
                        mergeDataToSheetRow(maps, row, srow);
                    }
                }
                using (FileStream fs = File.Create(filepath))
                {
                    workbook.Write(fs);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR::" + ex.Message + "\n" + ex.StackTrace);
                log.Error(ex);
                return false;
            }

            return true;
        }
        /// <summary>
        /// 根据字段将一行记录转换成IRow
        /// </summary>
        /// <param name="customAttrDBMapping">字段名称</param>
        /// <param name="row">一条记录</param>
        /// <param name="srow">IRow</param>
        protected void mergeDataToSheetRow(Dictionary<string, int> customAttrDBMapping, DataRow row, IRow srow)
        {
            int idx=0;
            foreach (KeyValuePair<string, int> kvpair in customAttrDBMapping)
            {
                if (kvpair.Value > 0)
                {
                    int k = kvpair.Value;
                    string value = row[k].ToString();
                    srow.CreateCell(idx).SetCellValue(value);
                }else{
                    srow.CreateCell(idx);
                }
                ++idx;
            }
        }
        /// <summary>
        /// 用于针对Excel的列名转换实现，1->A
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string numToExcelIndex(int value)
        {
            if (value < 1 || value > 18277)
            {
#if DEBUG
                System.Diagnostics.Debug.Assert(value>0 && value<18278);
#endif
                return null;
            }
            string rtn = string.Empty;
            List<int> iList = new List<int>();
            //To single Int
            while (value / 26 != 0 || value % 26 != 0)
            {
                iList.Add(value % 26);
                value /= 26;
            }
            //Change 0 To 26
            for (int j = 0; j < iList.Count - 1; j++)
            {
                if (iList[j] == 0)
                {
                    iList[j + 1] -= 1;
                    iList[j] = 26;
                }
            }
            //Remove 0 at last
            if (iList[iList.Count - 1] == 0)
            {
                iList.Remove(iList[iList.Count - 1]);
            }
            //To String
            for (int j = iList.Count - 1; j >= 0; j--)
            {
                char c = (char)(iList[j] + 64);
                rtn += c.ToString();
            }
            return rtn;
        }
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
    }
}
