using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Winner.Persistence;

namespace WebCore.Extension
{
    public static class ExcelHelper
    { 

        /// <summary>
        /// 设置导出格式
        /// </summary>
        private static DataTable SetExcelDataTable(DataTable dt, Dictionary<string, string> items)
        {
            if (items != null)
            {
                var i = 0;
                foreach (KeyValuePair<string, string> item in items)
                {
                    dt.Columns[i].ColumnName = item.Key;
                    dt.Columns[i].Caption = item.Value;
                    i++;
                }
            }
            return dt;
        }
         
        /// <summary>
        /// 输出Excel
        /// </summary>
        //public static IActionResult ExportExcel<T>(this Controller controller, string excelName,
        //    Dictionary<string, string> items, QueryInfo query)
        //{
        //    var dt = Ioc.Resolve<IApplicationService, T>().Execute<DataTable>(query);
        //    if (dt.Rows.Count > 0)
        //    {
        //        dt = SetExcelDataTable(dt, items);
        //        var file = new FileContentResult(Component.Extension.ExcelHelper.ExportExcel(dt),
        //            "applicationnd.openxmlformats-officedocument.spreadsheetml.sheet")
        //        {
        //            FileDownloadName = excelName
        //        };
        //        return file;
        //    }
        //    return null;
        //}
    }
}
