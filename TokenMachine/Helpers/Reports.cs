using log4net;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace TokenMachine.Helpers
{
    public class Reports
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        Access acc = new Access();


        public DataTable GetSales(DateTime from, DateTime to)
        {
            try
            {
                string cmd = @"select s.sales_code as InvoiceNo, s.sales_date as Date,  sales_type as SalesType, s.Total as Amount, Item as Particulars, s.customer_code as Id, s.customer_name as Name
                        from tbl_sales s
                        left outer join
                        (
                        SELECT sales_code, group_concat(concat(product_name, '-', sales_quantity, '-', amount) separator '\r\n' ) as Item
                         FROM trn_sales
                         group by sales_code
                        ) as i on i.sales_code = s.sales_code
                        where s.IsActive = 1 and s.machine_id = " + config.machine_id + " and date(sales_date)  between '" + from.ToString("yyyy-MM-dd") + "' and '" + to.ToString("yyyy-MM-dd") + "' order by s.sales_id";

                DataTable dt = acc.GetTable(cmd);
                return dt;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public DataTable GetSalesCustomer(DateTime from, DateTime to)
        {
            try
            {
                string cmd = $@"select  o.customer_code as Id, o.customer_name as Name, sum(o.total) as Amount , group_concat(item separator '\r\n' ) as Items  from (
                                    select s.customer_id, s.customer_code, s.customer_name, s.total, t.item from tbl_sales s
                                    inner join(
                                        select sales_code, group_concat(concat(product_name, '-', sales_quantity, '-', amount) separator '\r\n' ) as Item  from trn_sales group by sales_code) as t on t.sales_code = s.sales_code
                                    where s.IsActive = 1 and s.sales_type = 'Account' and s.machine_id = 1   and date(s.sales_date) between '{from.ToString("yyyy-MM-dd")}' and '{ to.ToString("yyyy-MM-dd")}') o
                                group by o.customer_id";

                DataTable dt = acc.GetTable(cmd);
                return dt;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public DataTable GetSalesProduct(DateTime from, DateTime to)
        {
            try
            {
                string cmd = @"select Product_Name, Price, sum(sales_quantity) as Quantity , sum(amount) as Amount
                            from trn_sales 
                            where IsActive = 1 and machine_id = " + config.machine_id + " and date(updatedon) between '" + from.ToString("yyyy-MM-dd") + "' and '" + to.ToString("yyyy-MM-dd") + "' group by product_id";
                DataTable dt = acc.GetTable(cmd);
                return dt;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public DataTable GetStockTray(DateTime from, DateTime to)
        {
            try
            {
                //string cmd = "";
                //if (to.Date >= DateTime.Now.Date)
                //{
                //    cmd = @"SELECT product_no as TrayNo , Product_Name, Price, Stock, CAST(Price * Stock AS DECIMAL) as Amount 
                //                from tbl_motor_settings s
                //                inner join mst_product p on p.product_id = s.product_id
                //                where s.IsActive = 1 and s.machine_id = " + config.machine_id;
                //}
                //else
                //{

                //    cmd = string.Format(@"select TrayNo , Product_Name, Price, Stock, CAST(Price * Stock  AS DECIMAL) as Amount from (

                //                                select TrayNo , product_id ,  Stock  from(
                //                                select product_no As TrayNo ,product_id ,  filled_quantity - cleared_quantity - sales_quantity as Stock , Sdate from(
                //                                select product_no, product_id,  sum(filled_quantity) as  filled_quantity ,  sum(cleared_quantity) as  cleared_quantity, sum(sales_quantity) as  sales_quantity , date(max(Sdate)) as Sdate from (

                //                                select product_no, product_id,  sum(filled_quantity) as  filled_quantity , 0 as cleared_quantity, 0 as sales_quantity, date(max(updatedon)) as Sdate
                //                                from trn_product 
                //                                where IsActive =1 and machine_id = {0} and date(updatedon) <= '{1}'
                //                                group by product_no, product_id

                //                                union all 

                //                                select product_no, product_id,  0 as filled_quantity, sum(cleared_quantity) as  cleared_quantity, 0 as sales_quantity, date(max(updatedon)) as Sdate
                //                                from trn_cleared 
                //                                where IsActive =1 and machine_id = {0} and date(updatedon) <=  '{1}'
                //                                group by product_no, product_id

                //                                union all 

                //                                select product_no, product_id,  0 as filled_quantity, 0 as cleared_quantity, sum(sales_quantity) as  sales_quantity, date(max(updatedon)) as Sdate
                //                                from trn_sales 
                //                                where IsActive =1 and machine_id = {0} and date(updatedon)  <=  '{1}'
                //                                group by product_no, product_id ) as M
                //                                group by M.product_no, product_id) as N 
                //                                order by N.Sdate desc
                //                                ) As A group by TrayNo) A 
                //                                inner join mst_product p on p.product_id = A.product_id
                //                                order by A.TrayNo", config.machine_id, to.ToString("yyyy-MM-dd"));
                //}

                string cmd = @"SELECT m.Product_No, p.Product_Name, p.Price , m.Capacity,  coalesce(good , 0) as Good, coalesce(expired , 0) as Expired, coalesce(stock , 0) as Stock , coalesce(e.Expired_on , ' ') as Expire_Date
                                 FROM tbl_motor_settings m
                                 inner join mst_product p on p.product_id = m.product_id  
                                 left outer join (
                                 select product_no, sum(quantity) as stock , sum( case when expired_on > now() then quantity else 0 end  ) as good , sum( case when expired_on <= now() then quantity else 0 end  ) as expired   
                                 from trn_stock
	                                where IsActive = 1 group by product_no
                                 ) as s on s.product_no = m.product_no
                                 
                                 left outer join (
									select product_no , GROUP_CONCAT(expired_on order by exp asc SEPARATOR ' ') as expired_on from (
										SELECT product_no, concat( date_format( expired_on , '%d-%m-%Y %h:%i %p' ) ,'-', CAST(sum(quantity) as char) , ' \r\n') as expired_on , expired_on as exp FROM trn_stock group by product_no , expired_on
									) as M group by M.product_no
                                 ) as e on e.product_no = m.product_no
                                 
                                 where m.IsActive = 1 and m.machine_id =1 and m.cabin_id = " + config.machine_id + " order by  m.product_no";
                DataTable dt = acc.GetTable(cmd);
                return dt;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public DataTable GetStockProduct(DateTime from, DateTime to)
        {
            try
            {
                string cmd = "";

                if (to.Date >= DateTime.Now.Date)
                {
                    cmd = @"SELECT Product_Name, Price, sum(Stock) as Stock, CAST(Price * sum( Stock) AS DECIMAL) as Amount 
                                from tbl_motor_settings s
                                inner join mst_product p on p.product_id = s.product_id
                                where s.IsActive = 1 and s.machine_id = " + config.machine_id + " group by p.product_id";
                }
                else
                {

                    cmd = string.Format(@"select Product_Name , Price , filled_quantity - cleared_quantity - sales_quantity as Stock , CAST( (filled_quantity - cleared_quantity - sales_quantity) * Price AS DECIMAL) as Amount  from(
                                                select   product_id,   sum(filled_quantity) as  filled_quantity ,  sum(cleared_quantity) as  cleared_quantity, sum(sales_quantity) as  sales_quantity   from (

                                                select   product_id,  sum(filled_quantity) as  filled_quantity , 0 as cleared_quantity, 0 as sales_quantity 
                                                from trn_product 
                                                where IsActive =1 and machine_id = {0} and date(updatedon) <= '{1}' group by  product_id
                                                union all 
                                                select  product_id,  0 as filled_quantity, sum(cleared_quantity) as  cleared_quantity, 0 as sales_quantity 
                                                from trn_cleared 
                                                where IsActive =1 and machine_id = {0} and date(updatedon) <=  '{1}' group by product_id
                                                union all 
                                                select  product_id,  0 as filled_quantity, 0 as cleared_quantity, sum(sales_quantity) as  sales_quantity 
                                                from trn_sales 
                                                where IsActive =1 and machine_id = {0} and date(updatedon)  <=  '{1}' group by product_id 

                                                ) as M
                                                group by  product_id) as N 
                                                inner join mst_product p on p.product_id = N.product_id", config.machine_id, to.ToString("yyyy-MM-dd"));
                }
                DataTable dt = acc.GetTable(cmd);
                return dt;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public DataTable GetRefill(DateTime from, DateTime to)
        {
            try
            {
                string cmd = @"SELECT  e.Emp_Name, p.updatedon as Date, Product_No, p.Product_Name, m.Price, Filled_Quantity as Qty, cast(m.price * Filled_Quantity as decimal) as Amount
                                    FROM  trn_product p
                                    left outer join mst_employee e on e.emp_id = p.updatedby
                                    inner join mst_product m on m.product_id = p.product_id
                                    where p.IsActive = 1 and p.machine_id = " + config.machine_id + " and date(p.updatedon) between '" + from.ToString("yyyy-MM-dd") + "' and '" + to.ToString("yyyy-MM-dd") + "'";
                DataTable dt = acc.GetTable(cmd);
                return dt;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public DataTable GetStockReturn(DateTime from, DateTime to)
        {
            try
            {
                string cmd = @"SELECT  e.Emp_Name, p.updatedon as Date, Product_No, p.Product_Name, m.Price, Cleared_Quantity as Qty, cast(m.price * Cleared_Quantity as decimal) as Amount
                                    FROM  trn_cleared p
                                    inner join mst_employee e on e.emp_id = p.updatedby
                                    inner join mst_product m on m.product_id = p.product_id
                                    where p.IsActive = 1 and p.machine_id =  " + config.machine_id + " and date(p.updatedon) between '" + from.ToString("yyyy-MM-dd") + "' and '" + to.ToString("yyyy-MM-dd") + "'";
                DataTable dt = acc.GetTable(cmd);
                return dt;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public DataTable GetCustomerList(DateTime from, DateTime to)
        {
            try
            {
                string cmd = @" SELECT  c.customer_code as ID ,  customer_name as Name ,  date(created_on) as Created FROM mst_customer where IsActive = 1 and date(created_on)  between '" + from.ToString("yyyy-MM-dd") + "' and '" + to.ToString("yyyy-MM-dd") + "'";
                DataTable dt = acc.GetTable(cmd);
                return dt;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public DataTable GetRechargeList(DateTime from, DateTime to)
        {
            try
            {
                string cmd = @" select c.customer_name as Customer, c.Mobileno,  date_format( recharge_date , '%d-%m-%Y %h:%i %p') as RechargeDate , recharge_amount as Amount 
                                    from trn_recharge r 
                                    inner join mst_customer c on r.customer_id = c.customer_id
                                    where r.customer_id > 0 and date(recharge_date) between '" + from.ToString("yyyy-MM-dd") + "' and '" + to.ToString("yyyy-MM-dd") + "' order by recharge_date";

                DataTable dt = acc.GetTable(cmd);
                return dt;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }

        public bool GenerateExcelReport(string path, DateTime from, DateTime to)
        {
            try
            {
                char[] albhabets = "abcdefghijklmnopqrstuvwxyz".ToUpper().ToCharArray();
                HSSFWorkbook wb = new HSSFWorkbook();

                int scol = 0;

                #region Style
                var font = wb.CreateFont();
                font.FontHeightInPoints = 12;
                font.FontName = "Calibri";
                font.IsBold = true;


                var style = wb.CreateCellStyle();
                style.WrapText = true;
                style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                style.SetFont(font);


                var sum_style = wb.CreateCellStyle();
                sum_style.WrapText = true;
                sum_style.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Right;
                sum_style.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                sum_style.SetFont(font);
                #endregion

                #region Sales Report

                ISheet sheet1 = wb.CreateSheet("Sales");
                DataTable dt = GetSales(from, to);

                int r = 0;
                IRow row = sheet1.CreateRow(r++);
                row.Height = (short)(row.Height * 3);
                ICell cell = row.CreateCell(0);
                cell.SetCellValue(("Sales Report between " + from.ToString("dd-MM-yyyy") + " and " + to.ToString("dd-MM-yyyy")).ToUpper());


                row = sheet1.CreateRow(r++);
                row.Height = (short)(row.Height * 1.5);

                int c = 0;
                cell = row.CreateCell(c);
                cell.SetCellValue("SNo");

                c++;
                List<int> maximumLengthForColumns = new List<int>();
                maximumLengthForColumns.Add(3);
                foreach (DataColumn dc in dt.Columns)
                {
                    cell = row.CreateCell(c);
                    cell.SetCellValue(dc.ColumnName);
                    maximumLengthForColumns.Add(dc.ColumnName.Length > 100 ? 100 : dc.ColumnName.Length);
                    c++;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    row = sheet1.CreateRow(r++);
                    row.CreateCell(0).SetCellValue(r - 2);


                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (!(dr[j] is DBNull))
                        {
                            if (dt.Columns[j].DataType == typeof(int))
                            {

                                row.CreateCell(j + 1).SetCellValue((int)dr[j]);
                            }
                            else if (dt.Columns[j].DataType == typeof(DateTime))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDateTime(dr[j]).ToString("dd-MM-yyyy hh:mm tt"));
                            }
                            else if (dt.Columns[j].DataType == typeof(decimal) || dt.Columns[j].DataType == typeof(double))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDouble(dr[j]));
                            }
                            else
                            {
                                row.CreateCell(j + 1).SetCellValue(dr[j].ToString());
                            }

                            maximumLengthForColumns[j + 1] = maximumLengthForColumns[j + 1] > dr[j].ToString().Length ? maximumLengthForColumns[j + 1] : dr[j].ToString().Length;
                        }
                        else
                        {
                            //  row.CreateCell(j + 1).SetCellValue("");
                        }
                    }
                }

                sheet1.GetRow(0).GetCell(0).CellStyle = style;
                sheet1.AddMergedRegion(new CellRangeAddress(0, 0, 0, dt.Columns.Count));
                sheet1.SetAutoFilter(new CellRangeAddress(1, 1, 0, dt.Columns.Count));
                sheet1.CreateFreezePane(0, 2);
                for (int i = 0; i < maximumLengthForColumns.Count; i++)
                {
                    sheet1.SetColumnWidth(i, ((maximumLengthForColumns[i] > 100 ? 100 : maximumLengthForColumns[i]) + 8) * 255);
                    sheet1.GetRow(1).GetCell(i).CellStyle = style;
                }

                if (r > 4)
                {
                    row = sheet1.CreateRow(r++);
                    row.Height = (short)(row.Height * 1.5);

                    scol = dt.Columns.IndexOf("Amount") + 1;
                    cell = row.CreateCell(scol);
                    cell.SetCellType(CellType.Formula);
                    cell.SetCellFormula(String.Format("SUM({0}3:{1}{2})", albhabets[scol], albhabets[scol], (r - 1)));
                    cell.CellStyle = sum_style;
                }

                #endregion

                #region Customer Recharge report

                ISheet sheet_customer = wb.CreateSheet("EmployeeWiseSales");

                dt = GetSalesCustomer(from, to);

                r = 0;
                row = sheet_customer.CreateRow(r++);
                row.Height = (short)(row.Height * 3);
                cell = row.CreateCell(0);
                cell.SetCellValue(("Employee Wise Sales Report -  " + from.ToString("dd-MM-yyyy") + " and " + to.ToString("dd-MM-yyyy")).ToUpper());


                row = sheet_customer.CreateRow(r++);
                row.Height = (short)(row.Height * 1.5);

                c = 0;
                cell = row.CreateCell(c);
                cell.SetCellValue("SNo");

                c++;
                maximumLengthForColumns = new List<int>();
                maximumLengthForColumns.Add(3);
                foreach (DataColumn dc in dt.Columns)
                {
                    cell = row.CreateCell(c);
                    cell.SetCellValue(dc.ColumnName);
                    maximumLengthForColumns.Add(dc.ColumnName.Length);
                    c++;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    row = sheet_customer.CreateRow(r++);
                    row.CreateCell(0).SetCellValue(r - 2);  

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (!(dr[j] is DBNull))
                        {
                            if (dt.Columns[j].DataType == typeof(int))
                            {

                                row.CreateCell(j + 1).SetCellValue((int)dr[j]);
                            }
                            else if (dt.Columns[j].DataType == typeof(DateTime))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDateTime(dr[j]).ToString("dd-MM-yyyy hh:mm tt"));
                            }
                            else if (dt.Columns[j].DataType == typeof(decimal) || dt.Columns[j].DataType == typeof(double))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDouble(dr[j]));
                            }
                            else
                            {
                                row.CreateCell(j + 1).SetCellValue(dr[j].ToString());
                            }

                            maximumLengthForColumns[j + 1] = maximumLengthForColumns[j + 1] > dr[j].ToString().Length ? maximumLengthForColumns[j + 1] : dr[j].ToString().Length;
                        }
                        else
                        {
                            //  row.CreateCell(j + 1).SetCellValue("");
                        }
                    }
                }

                sheet_customer.GetRow(0).GetCell(0).CellStyle = style;

                sheet_customer.AddMergedRegion(new CellRangeAddress(0, 0, 0, dt.Columns.Count));
                sheet_customer.SetAutoFilter(new CellRangeAddress(1, 1, 0, dt.Columns.Count));
                sheet_customer.CreateFreezePane(0, 2);

                for (int i = 0; i < maximumLengthForColumns.Count; i++)
                {
                    sheet_customer.SetColumnWidth(i, ((maximumLengthForColumns[i] > 100 ? 100 : maximumLengthForColumns[i]) + 8) * 255);
                    sheet_customer.GetRow(1).GetCell(i).CellStyle = style;
                }

                if (r > 4)
                {
                    row = sheet_customer.CreateRow(r++);
                    row.Height = (short)(row.Height * 1.5);

                    scol = dt.Columns.IndexOf("Amount") + 1;
                    cell = row.CreateCell(scol);
                    cell.SetCellType(CellType.Formula);
                    cell.SetCellFormula(String.Format("SUM({0}3:{1}{2})", albhabets[scol], albhabets[scol], (r - 1)));
                    cell.CellStyle = sum_style;
                }

                #endregion

                #region Produt Sales Report

                ISheet sheet2 = wb.CreateSheet("Sales-Product");
                dt = GetSalesProduct(from, to);
                r = 0;
                row = sheet2.CreateRow(r++);
                row.Height = (short)(row.Height * 3);
                cell = row.CreateCell(0);
                cell.SetCellValue(("Sales Report - Poduct Based between " + from.ToString("dd-MM-yyyy") + " and " + to.ToString("dd-MM-yyyy")).ToUpper());


                row = sheet2.CreateRow(r++);
                row.Height = (short)(row.Height * 1.5);

                c = 0;
                cell = row.CreateCell(c);
                cell.SetCellValue("SNo");

                c++;
                maximumLengthForColumns = new List<int>();
                maximumLengthForColumns.Add(3);
                foreach (DataColumn dc in dt.Columns)
                {
                    cell = row.CreateCell(c);
                    cell.SetCellValue(dc.ColumnName);
                    maximumLengthForColumns.Add(dc.ColumnName.Length);
                    c++;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    row = sheet2.CreateRow(r++);
                    row.CreateCell(0).SetCellValue(r - 2);

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (!(dr[j] is DBNull))
                        {
                            if (dt.Columns[j].DataType == typeof(int))
                            {

                                row.CreateCell(j + 1).SetCellValue((int)dr[j]);
                            }
                            else if (dt.Columns[j].DataType == typeof(DateTime))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDateTime(dr[j]).ToString("dd-MM-yyyy hh:mm tt"));
                            }
                            else if (dt.Columns[j].DataType == typeof(decimal) || dt.Columns[j].DataType == typeof(double))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDouble(dr[j]));
                            }
                            else
                            {
                                row.CreateCell(j + 1).SetCellValue(dr[j].ToString());
                            }

                            maximumLengthForColumns[j + 1] = maximumLengthForColumns[j + 1] > dr[j].ToString().Length ? maximumLengthForColumns[j + 1] : dr[j].ToString().Length;
                        }
                        else
                        {
                            //  row.CreateCell(j + 1).SetCellValue("");
                        }
                    }
                }

                sheet2.GetRow(0).GetCell(0).CellStyle = style;

                sheet2.AddMergedRegion(new CellRangeAddress(0, 0, 0, dt.Columns.Count));
                sheet2.SetAutoFilter(new CellRangeAddress(1, 1, 0, dt.Columns.Count));
                sheet2.CreateFreezePane(0, 2);

                for (int i = 0; i < maximumLengthForColumns.Count; i++)
                {
                    sheet2.SetColumnWidth(i, ((maximumLengthForColumns[i] > 100 ? 100 : maximumLengthForColumns[i]) + 8) * 255);
                    sheet2.GetRow(1).GetCell(i).CellStyle = style;
                }

                if (r > 4)
                {
                    row = sheet2.CreateRow(r++);
                    row.Height = (short)(row.Height * 1.5);

                    scol = dt.Columns.IndexOf("Amount") + 1;
                    cell = row.CreateCell(scol);
                    cell.SetCellType(CellType.Formula);
                    cell.SetCellFormula(String.Format("SUM({0}3:{1}{2})", albhabets[scol], albhabets[scol], (r - 1)));
                    cell.CellStyle = sum_style;
                }
                #endregion

                //#region Stock - Tray Report

                //ISheet sheet3 = wb.CreateSheet("Stock-Tray");
                //dt = GetStockTray(from, to);

                //r = 0;
                //row = sheet3.CreateRow(r++);
                //row.Height = (short)(row.Height * 3);
                //cell = row.CreateCell(0);
                //cell.SetCellValue(("Stock - Tray Based Report between " + from.ToString("dd-MM-yyyy") + " and " + to.ToString("dd-MM-yyyy")).ToUpper());


                //row = sheet3.CreateRow(r++);
                //row.Height = (short)(row.Height * 1.5);

                //c = 0;
                //cell = row.CreateCell(c);
                //cell.SetCellValue("SNo");

                //c++;
                //maximumLengthForColumns = new List<int>();
                //maximumLengthForColumns.Add(3);
                //foreach (DataColumn dc in dt.Columns)
                //{
                //    cell = row.CreateCell(c);
                //    cell.SetCellValue(dc.ColumnName);
                //    maximumLengthForColumns.Add(dc.ColumnName.Length);
                //    c++;
                //}

                //foreach (DataRow dr in dt.Rows)
                //{
                //    row = sheet3.CreateRow(r++);
                //    row.CreateCell(0).SetCellValue(r - 2);

                //    for (int j = 0; j < dt.Columns.Count; j++)
                //    {
                //        if (!(dr[j] is DBNull))
                //        {
                //            if (dt.Columns[j].DataType == typeof(int))
                //            {

                //                row.CreateCell(j + 1).SetCellValue((int)dr[j]);
                //            }
                //            else if (dt.Columns[j].DataType == typeof(DateTime))
                //            {
                //                row.CreateCell(j + 1).SetCellValue(Convert.ToDateTime(dr[j]).ToString("dd-MM-yyyy hh:mm tt"));
                //            }
                //            else if (dt.Columns[j].DataType == typeof(decimal) || dt.Columns[j].DataType == typeof(double))
                //            {
                //                row.CreateCell(j + 1).SetCellValue(Convert.ToDouble(dr[j]));
                //            }
                //            else
                //            {
                //                row.CreateCell(j + 1).SetCellValue(dr[j].ToString());
                //            }

                //            maximumLengthForColumns[j + 1] = maximumLengthForColumns[j + 1] > dr[j].ToString().Length ? maximumLengthForColumns[j + 1] : dr[j].ToString().Length;
                //        }
                //        else
                //        {
                //            //  row.CreateCell(j + 1).SetCellValue("");
                //        }
                //    }
                //}

                //sheet3.GetRow(0).GetCell(0).CellStyle = style;

                //sheet3.AddMergedRegion(new CellRangeAddress(0, 0, 0, dt.Columns.Count));
                //sheet3.SetAutoFilter(new CellRangeAddress(1, 1, 0, dt.Columns.Count));
                //sheet3.CreateFreezePane(0, 2);

                //for (int i = 0; i < maximumLengthForColumns.Count; i++)
                //{
                //    sheet3.SetColumnWidth(i, ((maximumLengthForColumns[i] > 100 ? 100 : maximumLengthForColumns[i]) + 8) * 255);
                //    sheet3.GetRow(1).GetCell(i).CellStyle = style;
                //}
                //if (r > 4)
                //{
                //    row = sheet3.CreateRow(r++);
                //    row.Height = (short)(row.Height * 1.5);

                //    scol = dt.Columns.IndexOf("Amount") + 1;
                //    if (scol > 0)
                //    {
                //        cell = row.CreateCell(scol);
                //        cell.SetCellType(CellType.Formula);
                //        cell.SetCellFormula(String.Format("SUM({0}3:{1}{2})", albhabets[scol], albhabets[scol], (r - 1)));
                //        cell.CellStyle = sum_style;
                //    }
                //}
                //#endregion

                //#region Stock - Product Report

                //ISheet sheet4 = wb.CreateSheet("Stock-Product");
                //dt = GetStockProduct(from, to);

                //r = 0;
                //row = sheet4.CreateRow(r++);
                //row.Height = (short)(row.Height * 3);
                //cell = row.CreateCell(0);
                //cell.SetCellValue(("Stock - Product Based Report between " + from.ToString("dd-MM-yyyy") + " and " + to.ToString("dd-MM-yyyy")).ToUpper());


                //row = sheet4.CreateRow(r++);
                //row.Height = (short)(row.Height * 1.5);

                //c = 0;
                //cell = row.CreateCell(c);
                //cell.SetCellValue("SNo");

                //c++;
                //maximumLengthForColumns = new List<int>();
                //maximumLengthForColumns.Add(3);
                //foreach (DataColumn dc in dt.Columns)
                //{
                //    cell = row.CreateCell(c);
                //    cell.SetCellValue(dc.ColumnName);
                //    maximumLengthForColumns.Add(dc.ColumnName.Length);
                //    c++;
                //}

                //foreach (DataRow dr in dt.Rows)
                //{
                //    row = sheet4.CreateRow(r++);
                //    row.CreateCell(0).SetCellValue(r - 2);

                //    for (int j = 0; j < dt.Columns.Count; j++)
                //    {
                //        if (!(dr[j] is DBNull))
                //        {
                //            if (dt.Columns[j].DataType == typeof(int))
                //            {

                //                row.CreateCell(j + 1).SetCellValue((int)dr[j]);
                //            }
                //            else if (dt.Columns[j].DataType == typeof(DateTime))
                //            {
                //                row.CreateCell(j + 1).SetCellValue(Convert.ToDateTime(dr[j]).ToString("dd-MM-yyyy hh:mm tt"));
                //            }
                //            else if (dt.Columns[j].DataType == typeof(decimal) || dt.Columns[j].DataType == typeof(double))
                //            {
                //                row.CreateCell(j + 1).SetCellValue(Convert.ToDouble(dr[j]));
                //            }
                //            else
                //            {
                //                row.CreateCell(j + 1).SetCellValue(dr[j].ToString());
                //            }

                //            maximumLengthForColumns[j + 1] = maximumLengthForColumns[j + 1] > dr[j].ToString().Length ? maximumLengthForColumns[j + 1] : dr[j].ToString().Length;
                //        }
                //        else
                //        {
                //            //  row.CreateCell(j + 1).SetCellValue("");
                //        }
                //    }
                //}

                //sheet4.GetRow(0).GetCell(0).CellStyle = style;

                //sheet4.AddMergedRegion(new CellRangeAddress(0, 0, 0, dt.Columns.Count));
                //sheet4.SetAutoFilter(new CellRangeAddress(1, 1, 0, dt.Columns.Count));
                //sheet4.CreateFreezePane(0, 2);

                //for (int i = 0; i < maximumLengthForColumns.Count; i++)
                //{
                //    sheet4.SetColumnWidth(i, ((maximumLengthForColumns[i] > 100 ? 100 : maximumLengthForColumns[i]) + 8) * 255);
                //    sheet4.GetRow(1).GetCell(i).CellStyle = style;
                //}

                //row = sheet4.CreateRow(r++);
                //row.Height = (short)(row.Height * 1.5);
                //if (r > 4)
                //{
                //    scol = dt.Columns.IndexOf("Amount") + 1;
                //    cell = row.CreateCell(scol);
                //    cell.SetCellType(CellType.Formula);
                //    cell.SetCellFormula(String.Format("SUM({0}3:{1}{2})", albhabets[scol], albhabets[scol], (r - 1)));
                //    cell.CellStyle = sum_style;
                //}
                //#endregion

                #region Refill Report


                ISheet sheet5 = wb.CreateSheet("Refill");
                dt = GetRefill(from, to);

                r = 0;
                row = sheet5.CreateRow(r++);
                row.Height = (short)(row.Height * 3);
                cell = row.CreateCell(0);
                cell.SetCellValue(("Refill Report between " + from.ToString("dd-MM-yyyy") + " and " + to.ToString("dd-MM-yyyy")).ToUpper());

                row = sheet5.CreateRow(r++);
                row.Height = (short)(row.Height * 1.5);

                c = 0;
                cell = row.CreateCell(c);
                cell.SetCellValue("SNo");

                c++;
                maximumLengthForColumns = new List<int>();
                maximumLengthForColumns.Add(3);
                foreach (DataColumn dc in dt.Columns)
                {
                    cell = row.CreateCell(c);
                    cell.SetCellValue(dc.ColumnName);
                    maximumLengthForColumns.Add(dc.ColumnName.Length);
                    c++;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    row = sheet5.CreateRow(r++);
                    row.CreateCell(0).SetCellValue(r - 2);

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (!(dr[j] is DBNull))
                        {
                            if (dt.Columns[j].DataType == typeof(int))
                            {

                                row.CreateCell(j + 1).SetCellValue((int)dr[j]);
                            }
                            else if (dt.Columns[j].DataType == typeof(DateTime))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDateTime(dr[j]).ToString("dd-MM-yyyy hh:mm tt"));
                            }
                            else if (dt.Columns[j].DataType == typeof(decimal) || dt.Columns[j].DataType == typeof(double))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDouble(dr[j]));
                            }
                            else
                            {
                                row.CreateCell(j + 1).SetCellValue(dr[j].ToString());
                            }

                            maximumLengthForColumns[j + 1] = maximumLengthForColumns[j + 1] > dr[j].ToString().Length ? maximumLengthForColumns[j + 1] : dr[j].ToString().Length;
                        }
                        else
                        {
                            //  row.CreateCell(j + 1).SetCellValue("");
                        }
                    }
                }

                sheet5.GetRow(0).GetCell(0).CellStyle = style;

                sheet5.AddMergedRegion(new CellRangeAddress(0, 0, 0, dt.Columns.Count));
                sheet5.SetAutoFilter(new CellRangeAddress(1, 1, 0, dt.Columns.Count));
                sheet5.CreateFreezePane(0, 2);

                for (int i = 0; i < maximumLengthForColumns.Count; i++)
                {
                    sheet5.SetColumnWidth(i, ((maximumLengthForColumns[i] > 100 ? 100 : maximumLengthForColumns[i]) + 8) * 255);
                    sheet5.GetRow(1).GetCell(i).CellStyle = style;
                }

                if (r > 4)
                {
                    row = sheet5.CreateRow(r++);
                    row.Height = (short)(row.Height * 1.5);

                    scol = dt.Columns.IndexOf("Amount") + 1;
                    cell = row.CreateCell(scol);
                    cell.SetCellType(CellType.Formula);
                    cell.SetCellFormula(String.Format("SUM({0}3:{1}{2})", albhabets[scol], albhabets[scol], (r - 1)));
                    cell.CellStyle = sum_style;
                }
                #endregion

                #region Cleared Report

                ISheet sheet6 = wb.CreateSheet("Stock-Return");
                dt = GetStockReturn(from, to);
                r = 0;
                row = sheet6.CreateRow(r++);
                row.Height = (short)(row.Height * 3);
                cell = row.CreateCell(0);
                cell.SetCellValue(("Stock - Return Report between " + from.ToString("dd-MM-yyyy") + " and " + to.ToString("dd-MM-yyyy")).ToUpper());


                row = sheet6.CreateRow(r++);
                row.Height = (short)(row.Height * 1.5);

                c = 0;
                cell = row.CreateCell(c);
                cell.SetCellValue("SNo");

                c++;
                maximumLengthForColumns = new List<int>();
                maximumLengthForColumns.Add(3);
                foreach (DataColumn dc in dt.Columns)
                {
                    cell = row.CreateCell(c);
                    cell.SetCellValue(dc.ColumnName);
                    maximumLengthForColumns.Add(dc.ColumnName.Length);
                    c++;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    row = sheet6.CreateRow(r++);
                    row.CreateCell(0).SetCellValue(r - 2);

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (!(dr[j] is DBNull))
                        {
                            if (dt.Columns[j].DataType == typeof(int))
                            {

                                row.CreateCell(j + 1).SetCellValue((int)dr[j]);
                            }
                            else if (dt.Columns[j].DataType == typeof(DateTime))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDateTime(dr[j]).ToString("dd-MM-yyyy hh:mm tt"));
                            }
                            else if (dt.Columns[j].DataType == typeof(decimal) || dt.Columns[j].DataType == typeof(double))
                            {
                                row.CreateCell(j + 1).SetCellValue(Convert.ToDouble(dr[j]));
                            }
                            else
                            {
                                row.CreateCell(j + 1).SetCellValue(dr[j].ToString());
                            }

                            maximumLengthForColumns[j + 1] = maximumLengthForColumns[j + 1] > dr[j].ToString().Length ? maximumLengthForColumns[j + 1] : dr[j].ToString().Length;
                        }
                        else
                        {
                            //  row.CreateCell(j + 1).SetCellValue("");
                        }
                    }
                }

                sheet6.GetRow(0).GetCell(0).CellStyle = style;

                sheet6.AddMergedRegion(new CellRangeAddress(0, 0, 0, dt.Columns.Count));
                sheet6.SetAutoFilter(new CellRangeAddress(1, 1, 0, dt.Columns.Count));
                sheet6.CreateFreezePane(0, 2);

                for (int i = 0; i < maximumLengthForColumns.Count; i++)
                {
                    sheet6.SetColumnWidth(i, ((maximumLengthForColumns[i] > 240 ? 240 : maximumLengthForColumns[i]) + 8) * 255);
                    sheet6.GetRow(1).GetCell(i).CellStyle = style;
                }

                if (r > 4)
                {
                    row = sheet6.CreateRow(r++);
                    row.Height = (short)(row.Height * 1.5);

                    scol = dt.Columns.IndexOf("Amount") + 1;
                    cell = row.CreateCell(scol);
                    cell.SetCellType(CellType.Formula);
                    cell.SetCellFormula(String.Format("SUM({0}3:{1}{2})", albhabets[scol], albhabets[scol], (r - 1)));
                    cell.CellStyle = sum_style;
                }
                #endregion



                HSSFFormulaEvaluator.EvaluateAllFormulaCells(wb);

                FileStream file = new FileStream(path, FileMode.Create);
                wb.Write(file);
                file.Close();

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        public string EmailExcelReport(DateTime from, DateTime to, string filename = "")
        {
            try
            {
                log.Info("Report generation started");
                string path = AppDomain.CurrentDomain.BaseDirectory + "Reports\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string Location = "", Machine_code = "", cmpName = "";
                cmpName = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;

                string cmd = "SELECT machine_code, display_company_name , location FROM mst_machine_details where machine_id = " + config.machine_id;
                DataTable dt_machine_info = acc.GetTable(cmd);
                if (dt_machine_info.Rows.Count > 0)
                {
                    Location = dt_machine_info.Rows[0]["location"].ToString();
                    cmpName = dt_machine_info.Rows[0]["display_company_name"].ToString();
                    Machine_code = dt_machine_info.Rows[0]["machine_code"].ToString();
                }

                if (filename.Length > 1)
                {
                    path = path + filename;
                }
                else
                {
                    path = path + "AutomatedReport_Date_" + to.ToString("dd-MM-yyyy") + ".xls";
                }


                if (GenerateExcelReport(path, from, to))
                {


                    string smtp_server = "secure225.servconfig.com", user_id = "reports@betaautomation.com", pwd = "Beta@786";
                    int smtp_port = 587;
                    bool is_ssl = true;

                    cmd = "SELECT smtp_server,smtp_port,is_ssl,user_id,pwd FROM  tbl_email_settings where IsActive = 1 and machine_id = " + config.machine_id;
                    DataTable dt_settings = acc.GetTable(cmd);
                    if (dt_settings.Rows.Count > 0)
                    {
                        DataRow dr = dt_settings.Rows[0];
                        smtp_server = dr["smtp_server"].ToString().Trim();
                        smtp_port = Convert.ToInt32(dr["smtp_port"].ToString().Trim());
                        is_ssl = Convert.ToBoolean(dr["is_ssl"]);
                        user_id = dr["user_id"].ToString().Trim();
                        pwd = dr["pwd"].ToString().Trim();
                    }



                    DataTable dt_emails = acc.GetTable("SELECT email_id FROM tbl_emails where IsActive = 1 and machine_id = " + config.machine_id);
                    if (dt_emails.Rows.Count > 0)
                    {
                        MailMessage newmsg = new MailMessage();

                        foreach (DataRow dr in dt_emails.Rows)
                        {
                            newmsg.To.Add(dr[0].ToString());
                        }


                        newmsg.Subject = "Vending Machine Report - " + Machine_code;
                        newmsg.Body = "Hello \r\n \tHere attached reports for Vending machine of " + cmpName + ", Machine Code = " + Machine_code + " , Location = " + Location + " , period of " + from.ToString("dd-MM-yyyy") + " to " + to.ToString("dd-MM-yyyy");

                        //For File Attachment, more file can also be attached
                        newmsg.Attachments.Add(new Attachment(path));



                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        newmsg.From = new MailAddress(user_id);
                        SmtpClient smtp = new SmtpClient(smtp_server, smtp_port);
                        smtp.UseDefaultCredentials = false;
                        smtp.EnableSsl = is_ssl;
                        smtp.Credentials = new NetworkCredential(user_id, pwd);
                        smtp.Send(newmsg);
                        log.Info("Mail send successfully to " + newmsg.To.ToString());
                        return "Mail send successfully to " + newmsg.To.ToString();
                    }
                    else
                    {
                        log.Info("To address not found");
                        return "To address not found";
                    }
                }
                else
                {
                    return "Error in generating excel report";
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return "Error : " + ex.Message;
            }
        }

        public void SendMachineOutoffOrder()
        {
            try
            {
                string Location = "", Machine_code = "", cmpName = "";
                cmpName = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;

                string cmd = "SELECT machine_code, display_company_name , location FROM mst_machine_details where machine_id = " + config.machine_id;
                DataTable dt_machine_info = acc.GetTable(cmd);
                if (dt_machine_info.Rows.Count > 0)
                {
                    Location = dt_machine_info.Rows[0]["location"].ToString();
                    cmpName = dt_machine_info.Rows[0]["display_company_name"].ToString();
                    Machine_code = dt_machine_info.Rows[0]["machine_code"].ToString();
                }


                string smtp_server = "secure225.servconfig.com", user_id = "reports@betaautomation.com", pwd = "Beta@786";
                int smtp_port = 587;
                bool is_ssl = true;

                cmd = "SELECT smtp_server,smtp_port,is_ssl,user_id,pwd FROM  tbl_email_settings where IsActive = 1 and machine_id = " + config.machine_id;
                DataTable dt_settings = acc.GetTable(cmd);
                if (dt_settings.Rows.Count > 0)
                {
                    DataRow dr = dt_settings.Rows[0];
                    smtp_server = dr["smtp_server"].ToString().Trim();
                    smtp_port = Convert.ToInt32(dr["smtp_port"].ToString().Trim());
                    is_ssl = Convert.ToBoolean(dr["is_ssl"]);
                    user_id = dr["user_id"].ToString().Trim();
                    pwd = dr["pwd"].ToString().Trim();
                }



                DataTable dt_emails = acc.GetTable("SELECT email_id FROM tbl_emails where IsActive = 1 and machine_id = " + config.machine_id);
                if (dt_emails.Rows.Count > 0)
                {
                    MailMessage newmsg = new MailMessage();

                    foreach (DataRow dr in dt_emails.Rows)
                    {
                        newmsg.To.Add(dr[0].ToString());
                    }


                    newmsg.Subject = "Vending Machine (" + Machine_code + ") out of order";
                    newmsg.Body = @"Hello \r\n \t Vending Machine out of order at " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt") +
                                   "\r\n Machine code = " + Machine_code +
                                   "\r\n Location = " + Location;

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    newmsg.From = new MailAddress(user_id);
                    SmtpClient smtp = new SmtpClient(smtp_server, smtp_port);
                    smtp.UseDefaultCredentials = false;
                    smtp.EnableSsl = is_ssl;
                    smtp.Credentials = new NetworkCredential(user_id, pwd);
                    smtp.Send(newmsg);
                    log.Info("Mail send successfully to " + newmsg.To.ToString());
                }
                else
                {
                    log.Info("To address not found");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public async void SendMail(string Subject, string body)
        {
            try
            {
                string Location = "", Machine_code = "", cmpName = "";
                cmpName = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;

                string cmd = "SELECT machine_code, display_company_name , location FROM mst_machine_details where machine_id = " + config.machine_id;
                DataTable dt_machine_info = acc.GetTable(cmd);
                if (dt_machine_info.Rows.Count > 0)
                {
                    Location = dt_machine_info.Rows[0]["location"].ToString();
                    cmpName = dt_machine_info.Rows[0]["display_company_name"].ToString();
                    Machine_code = dt_machine_info.Rows[0]["machine_code"].ToString();
                }


                string smtp_server = "secure225.servconfig.com", user_id = "reports@betaautomation.com", pwd = "Beta@786";
                int smtp_port = 587;
                bool is_ssl = true;

                cmd = "SELECT smtp_server,smtp_port,is_ssl,user_id,pwd FROM  tbl_email_settings where IsActive = 1 and machine_id = " + config.machine_id;
                DataTable dt_settings = acc.GetTable(cmd);
                if (dt_settings.Rows.Count > 0)
                {
                    DataRow dr = dt_settings.Rows[0];
                    smtp_server = dr["smtp_server"].ToString().Trim();
                    smtp_port = Convert.ToInt32(dr["smtp_port"].ToString().Trim());
                    is_ssl = Convert.ToBoolean(dr["is_ssl"]);
                    user_id = dr["user_id"].ToString().Trim();
                    pwd = dr["pwd"].ToString().Trim();
                }



                DataTable dt_emails = acc.GetTable("SELECT email_id FROM tbl_emails where IsActive = 1 and machine_id = " + config.machine_id);
                if (dt_emails.Rows.Count > 0)
                {
                    MailMessage newmsg = new MailMessage();

                    foreach (DataRow dr in dt_emails.Rows)
                    {
                        newmsg.To.Add(dr[0].ToString());
                    }


                    newmsg.Subject = Subject;
                    newmsg.Body = body +
                                   "\r\n Machine code = " + Machine_code +
                                   "\r\n Location = " + Location;

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    newmsg.From = new MailAddress(user_id);
                    SmtpClient smtp = new SmtpClient(smtp_server, smtp_port);
                    smtp.UseDefaultCredentials = false;
                    smtp.EnableSsl = is_ssl;
                    smtp.Credentials = new NetworkCredential(user_id, pwd);
                    await smtp.SendMailAsync(newmsg);
                    log.Info("Mail send successfully to " + newmsg.To.ToString() + "\n\t\tSub : " + Subject + "\n\t\tBody : " + body);
                }
                else
                {
                    log.Info("To address not found");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

    }
}
