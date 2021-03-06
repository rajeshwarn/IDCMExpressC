﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IDCM.Data.Base;
using IDCM.Data.Base.Utils;
using IDCM.Data;
using IDCM.Service.Common;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace IDCM.Service.DataTransfer
{
    class XMLExporter
    {
        /// <summary>
        /// 根据历史查询条件导出目标文本数据集
        /// @author JiahaiWu
        /// 通过getValidViewDBMapping()方法获取已经被缓存的用户浏览字段~数据库字段位序的映射关系；
        /// 然后对历史查询条件进行限定批量长度的轮询和导出转换操作。
        /// 本数据导出方式使用静态分隔符策略。
        /// 请注意对存储数据值中存在等同分隔符情形，尚未考虑额外处理策略，使用不当将造成导出不规范的数据文件。
        /// @note 更进一步支持转移字符转换的模式有待补充。
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="cmdstr"></param>
        /// <param name="tcount"></param>
        /// <param name="spliter"></param>
        /// <returns></returns>
        public bool exportXML(DataSourceMHub datasource, string filepath, string cmdstr, int tcount)
        {
            try
            {
                StringBuilder strbuilder = new StringBuilder();
                int count = 0;
                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    strbuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n");
                    strbuilder.Append("<strains>\r\n");
                    Dictionary<string, int> maps = LocalRecordMHub.getCustomAttrDBMapping(datasource);
                    ///////////////////
                    int offset = 0;
                    int stepLen = SysConstants.EXPORT_PAGING_COUNT;
                    while (offset < tcount)
                    {
                        int lcount = tcount - offset > stepLen ? stepLen : tcount - offset;
                        DataTable table = LocalRecordMHub.queryCTDRecordByHistSQL(datasource, cmdstr, lcount, offset);
                        foreach (DataRow row in table.Rows)
                        {
                            XmlElement xmlEle = convertToXML(xmlDoc, maps, row);
                            strbuilder.Append(xmlEle.OuterXml).Append("\r\n");
                            /////////////
                            if (++count % 100 == 0)
                            {
                                Byte[] info = new UTF8Encoding(true).GetBytes(strbuilder.ToString());
                                BinaryWriter bw = new BinaryWriter(fs);
                                fs.Write(info, 0, info.Length);
                                strbuilder.Length = 0;
                            }
                        }   
                        offset += lcount;
                    }
                    strbuilder.Append("</strains>");
                    if (strbuilder.Length > 0)
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(strbuilder.ToString());
                        BinaryWriter bw = new BinaryWriter(fs);
                        fs.Write(info, 0, info.Length);
                        strbuilder.Length = 0;
                    }
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
        /// 以Excle导出数据，数据源DataGridViewSelectedRowCollection
        /// </summary>
        /// <param name="datasource">DataSourceMHub句柄，主要封装WorkSpaceManager</param>
        /// <param name="filepath">导出路径</param>
        /// <param name="selectedRows">数据源</param>
        /// <returns></returns>
        internal bool exportXML(DataSourceMHub datasource, string filepath, DataGridViewSelectedRowCollection selectedRows)
        {
            try
            {
                StringBuilder strbuilder = new StringBuilder();
                int count = 0;
                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    strbuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n");
                    strbuilder.Append("<strains>\r\n");
                    Dictionary<string, int> maps = LocalRecordMHub.getCustomAttrDBMapping(datasource);
                    ///////////////////
                    for (int ridx = selectedRows.Count - 1; ridx >= 0; ridx--)
                    {
                        DataGridViewRow dgvRow = selectedRows[ridx];
                        string recordId = dgvRow.Cells[CTDRecordA.CTD_RID].Value as string;
                        DataTable table = LocalRecordMHub.queryCTDRecord(datasource, null, recordId);
                        foreach (DataRow row in table.Rows)
                        {
                            XmlElement xmlEle = convertToXML(xmlDoc, maps, row);
                            strbuilder.Append(xmlEle.OuterXml).Append("\r\n");
                        }
                        if (++count % 100 == 0)
                        {
                            Byte[] info = new UTF8Encoding(true).GetBytes(strbuilder.ToString());
                            BinaryWriter bw = new BinaryWriter(fs);
                            fs.Write(info, 0, info.Length);
                            strbuilder.Length = 0;
                        }
                    }
                    strbuilder.Append("</strains>");
                    if (strbuilder.Length > 0)
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(strbuilder.ToString());
                        BinaryWriter bw = new BinaryWriter(fs);
                        fs.Write(info, 0, info.Length);
                        strbuilder.Length = 0;
                    }
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
        /// 根据字段将一行记录转换成xmlElement
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="maps"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private static XmlElement convertToXML(XmlDocument xmlDoc,Dictionary<string, int> maps, DataRow row)
        {
            XmlElement strainEle = xmlDoc.CreateElement("strain");
            foreach (KeyValuePair<string,int> mapEntry in maps)
            {
                XmlElement attrEle = xmlDoc.CreateElement(mapEntry.Key);
                attrEle.InnerText = row[mapEntry.Value].ToString();
                strainEle.AppendChild(attrEle);
            }
            return strainEle;
        }
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();  
    }
}
