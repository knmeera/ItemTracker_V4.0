using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Project.Data.Models;
using ERP.Project.Data.Context;
using System.IO;
using System.Data.Entity;
using static ClassDemo.Controllers.BaseController;
using System.Data.OleDb;
using System.Web.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using PagedList;
using PagedList.Mvc;


//dummycomment
namespace ClassDemo.Controllers
{
    [IsAdmin]
    public class HomeController : BaseController
    {

        ProjectManagementDbContext db = new ProjectManagementDbContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult ItemsList(int? page)
        {
            TrackerItem ti = new TrackerItem();

            ViewBag.Project = new SelectList(db.projects, "ProjectId", "ProjectName");
            ViewBag.CategoryVB = new SelectList(db.ItemCategories, "CategoryId", "CategoryName");
            ViewBag.ItemTypeVB = new SelectList(db.ItemTypes, "ItemTypeId", "ItemName");
            ViewBag.ItemStatusVB = new SelectList(db.ItemStatuses, "ItemStatusId", "ItemStatusName");
            ViewBag.ItemPriorityVB = new SelectList(db.ItemPriorities, "PriorityId", "PriorityName");
            var user = Session["UserName"].ToString();
            var res = db.TrackerItems.Include(m => m.project).Include(m => m.ItemCategory).Include(m => m.ItemType).Include(m => m.ItemStatus).Include(m => m.ItemPriority).Where(m => m.AssignedTo == user || user == "admin").ToList().OrderByDescending(m => m.ItemId).ToPagedList(page ?? 1, 7);
            return View(res);

        }
        [HttpPost]
        public ActionResult ItemsList(TrackerItem ti, int? page)
        {
            TempData["items"] = ti;
            ViewBag.Project = new SelectList(db.projects, "ProjectId", "ProjectName");
            ViewBag.CategoryVB = new SelectList(db.ItemCategories, "CategoryId", "CategoryName");
            ViewBag.ItemTypeVB = new SelectList(db.ItemTypes, "ItemTypeId", "ItemName");
            ViewBag.ItemStatusVB = new SelectList(db.ItemStatuses, "ItemStatusId", "ItemStatusName");
            ViewBag.ItemPriorityVB = new SelectList(db.ItemPriorities, "PriorityId", "PriorityName");

            var items = db.TrackerItems.Where(m => m.ItemCategoryId == ti.ItemCategoryId || m.ItemStatusId == ti.ItemStatusId || m.ItemTypeId == ti.ItemTypeId || m.ItemProjectId == ti.ItemProjectId || m.ItemPriorityId == ti.ItemPriorityId).ToList().OrderByDescending(m => m.ItemId).ToPagedList(page ?? 1, 7);

            return View(items);
        }
        public ActionResult CreateItem()
        {
            //ViewBag.ItemId = db.TrackerItems.Max(p => p.ItemId) + 1;
            int dat = db.TrackerItems.Select(m => m.ItemId).Count();
            if (dat == 0)
            {
                ViewBag.ItemId = 1;
            }
            else
            {
                ViewBag.ItemId = db.TrackerItems.Max(p => p.ItemId) + 1;

            }

            ViewBag.Project = new SelectList(db.projects, "ProjectId", "ProjectName");
            ViewBag.CategoryVB = new SelectList(db.ItemCategories, "CategoryId", "CategoryName");
            ViewBag.ItemTypeVB = new SelectList(db.ItemTypes, "ItemTypeId", "ItemName");
            ViewBag.ItemStatusVB = new SelectList(db.ItemStatuses, "ItemStatusId", "ItemStatusName");
            ViewBag.ItemPriorityVB = new SelectList(db.ItemPriorities, "PriorityId", "PriorityName");
            TrackerItem itm = new TrackerItem();
            return View(itm);
        }
        [HttpPost]
        public ActionResult CreateItem(TrackerItem itm, HttpPostedFileBase file)
        {

            var fileName = String.Empty;
            if (file != null && file.ContentLength > 0)
            {
                fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/upload_attachements"), fileName);
                file.SaveAs(path);
            }

            if (ModelState.IsValid)
            {
                db.TrackerItems.Add(itm);
                db.SaveChanges();
                return RedirectToAction("ItemsList", "Home");
            }

            return View();
        }
        public ActionResult Edit(int ID, string Name)
        {
            if ((db.TrackerItems.Any(m => m.ItemId == ID && m.AssignedTo == Name) && Session["UserName"].ToString() == Name) || Session["UserName"].ToString() == "admin")
            {

                ViewBag.Project = new SelectList(db.projects, "ProjectId", "ProjectName");
                ViewBag.CategoryVB = new SelectList(db.ItemCategories, "CategoryId", "CategoryName");
                ViewBag.ItemTypeVB = new SelectList(db.ItemTypes, "ItemTypeId", "ItemName");
                ViewBag.ItemStatusVB = new SelectList(db.ItemStatuses, "ItemStatusId", "ItemStatusName");
                ViewBag.ItemPriorityVB = new SelectList(db.ItemPriorities, "PriorityId", "PriorityName");

                TrackerItem item = db.TrackerItems.Include(m => m.ItemCategory).Include(m => m.ItemPriority).Include(m => m.ItemStatus).Include(m => m.project).Include(m => m.ItemType).SingleOrDefault(x => x.ItemId == ID);
                return View(item);

            }
            else
            {
                return RedirectToAction("UnauthorisedUser", "Login");
            }
        }
        [HttpPost]
        public ActionResult Edit(TrackerItem itm)
        {

            db.Entry(itm).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("ItemsList");
        }

        public ActionResult DetailsView(int Id, string Name)
        {
            if ((db.TrackerItems.Any(m => m.ItemId == Id && m.AssignedTo == Name) && Session["UserName"].ToString() == Name) || Session["UserName"].ToString() == "admin")
            {

                TrackerItem itm = db.TrackerItems.Find(Id);

                return View(itm);

            }
            else
            {
                return RedirectToAction("UnauthorisedUser", "Login");
            }

        }
        [HttpPost]
        public ActionResult DetailsView(TrackerItem vm)
        {
            return RedirectToAction("Edit");
        }


        public ActionResult Delete(int Id)
        {
            try
            {
                TrackerItem itm = db.TrackerItems.Find(Id);
                db.TrackerItems.Remove(itm);
                db.SaveChanges();
            }
            catch
            {

            }
            return RedirectToAction("ItemsList");
        }


        //[HttpPost]
        //public ActionResult Excel(TrackerItem vm)
        //{
        //    try
        //    {
        //        //cm.ValidateRole(ref IsCreateActivityHead, ref IsAdministrator, ref IsFinanceHead);//cm.CheckTheAdminType(ref IsCreateActivityHead, ref IsAdministrator, ref IsFinanceHead);
        //        //vm.IsCreateActivityHead = IsCreateActivityHead;
        //        //vm.IsAdministrator = IsAdministrator;
        //        //vm.IsFinanceAdmin = IsFinanceHead;

        //        if (ModelState.IsValid && ((System.Web.HttpPostedFileBase)(vm.FileToUpload)) != null)
        //        {
        //            ViewBag.Status = ExcelToDataBase(((System.Web.HttpPostedFileBase)(vm.FileToUpload)));

        //            return RedirectToAction("Excel");
        //        }
        //        else if (((System.Web.HttpPostedFileBase)(vm.FileToUpload)) == null)
        //        {
        //            ViewBag.Status = "<span class='errorMessage'>Please select excel sheet</span>";
        //            return View(vm);
        //        }
        //        else
        //        {
        //            return View(vm);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Status = "File/Files uploading Failed. Reason:" + ex.Message.ToString();
        //    }
        //    return View(vm);

        //}
        //public string ExcelToDataBase(HttpPostedFileBase file)
        //{
        //    DataTable dt = new DataTable();
        //    using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(file.InputStream, false))
        //    {

        //        WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
        //        IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
        //        string relationshipId = sheets.First().Id.Value;
        //        WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
        //        Worksheet workSheet = worksheetPart.Worksheet;
        //        SheetData sheetData = workSheet.GetFirstChild<SheetData>();
        //        IEnumerable<Row> rows = sheetData.Descendants<Row>();

        //        foreach (Cell cell in rows.ElementAt(0))
        //        {
        //            dt.Columns.Add(GetCellValue(spreadSheetDocument, cell));
        //        }
        //        int dontInsertHeader = 0;
        //        foreach (Row row in rows) //this will also include your header row...
        //        {
        //            TrackerItem tracks = new TrackerItem();
        //            DataRow tempRow = dt.NewRow();
        //            int columnIndex = 0;
        //            //bool isEmpAlreadyExist = false;
        //            //int empRowID = -1;
        //            foreach (Cell cell in row.Descendants<Cell>())
        //            {
        //                // Gets the column index of the cell with data
        //                int cellColumnIndex = (int)GetColumnIndexFromName(GetColumnName(cell.CellReference));
        //                cellColumnIndex--; //zero based index
        //                if (columnIndex < cellColumnIndex)
        //                {
        //                    do
        //                    {
        //                        tempRow[columnIndex] = ""; //Insert blank data here;


        //                        columnIndex++;
        //                    }
        //                    while (columnIndex < cellColumnIndex);
        //                }
        //                tempRow[columnIndex] = GetCellValue(spreadSheetDocument, cell);

        //                columnIndex++;
        //            }
        //            //insert into database
        //            if (dontInsertHeader > 0)
        //            {
        //                tracks.ItemSummary = tempRow[0].ToString();
        //                tracks.ItemCreatedDate = tempRow[1].ToString();
        //                tracks.AssignedDate = tempRow[2].ToString();
        //                tracks.ItemEndDate = tempRow[3].ToString();
        //                tracks.WorkCompleted = tempRow[4].ToString();
        //                tracks.CreatedBy = tempRow[5].ToString();
        //                tracks.Owner = tempRow[6].ToString();
        //                tracks.Impact = tempRow[7].ToString();
        //                tracks.Resolution = tempRow[8].ToString();
        //                tracks.ResolvedDate = tempRow[9].ToString();
        //                tracks.AttachmentPath = tempRow[10].ToString();
        //                tracks.ParentId = Convert.ToInt32(tempRow[11]);
        //                tracks.AssignedTo = tempRow[12].ToString();
        //                tracks.Subjet = tempRow[13].ToString();
        //                tracks.ItemProjectId = Convert.ToInt32(tempRow[14]);
        //                tracks.ItemCategoryId = Convert.ToInt32(tempRow[15]);
        //                tracks.ItemTypeId = Convert.ToInt32(tempRow[16]);
        //                tracks.ItemPriorityId = Convert.ToInt32(tempRow[17]);
        //                tracks.ItemStatusId = Convert.ToInt32(tempRow[18]);

        //                db.TrackerItems.Add(tracks);


        //            }

        //            dontInsertHeader++;
        //        }
        //        db.SaveChanges();

        //    }

        //    return "";
        //}
        //public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        //{
        //    SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
        //    if (cell.CellValue == null)
        //    {
        //        return "";
        //    }
        //    string value = cell.CellValue.InnerXml;
        //    if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        //    {
        //        return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
        //    }
        //    else
        //    {
        //        return value;
        //    }
        //}
        //public static int? GetColumnIndexFromName(string columnName)
        //{

        //    //return columnIndex;
        //    string name = columnName;
        //    int number = 0;
        //    int pow = 1;
        //    for (int i = name.Length - 1; i >= 0; i--)
        //    {
        //        number += (name[i] - 'A' + 1) * pow;
        //        pow *= 26;
        //    }
        //    return number;
        //}

        //public static string GetColumnName(string cellReference)
        //{
        //    // Create a regular expression to match the column name portion of the cell name.
        //    Regex regex = new Regex("[A-Za-z]+");
        //    Match match = regex.Match(cellReference);
        //    return match.Value;
        //}

        //private bool ValidateForm(TrackerItem itm)
        //{

        //    if (itm.ItemId == 0)
        //        ModelState.AddModelError("ItemId", "Please enter Item Id.");
        //    if (itm.ItemSummary == null)
        //        ModelState.AddModelError("ItemSummary", "Please enter ItemSummary");
        //    else if (itm.ItemCategoryId == 0)
        //        ModelState.AddModelError("ItemCategoryId", "Please enter ItemCategory");
        //    else if (itm.ItemTypeId == 0)
        //        ModelState.AddModelError("ItemType", "Please enter ItemCategory");
        //    else if (itm.ItemPriorityId == 0)
        //        ModelState.AddModelError("Priority", "Please enter Priority");
        //    else if (itm.ItemCreatedDate == null)
        //        ModelState.AddModelError("ItemCreatedDate", "Please enter Date");
        //    else if (itm.CreatedBy == null)
        //        ModelState.AddModelError("CreatedBy", "Please enter CreatedBy");
        //    else if (itm.Owner == null)
        //        ModelState.AddModelError("Owner", "Please enter Owner");
        //    else if (itm.Impact == null)
        //        ModelState.AddModelError("Impact", "Please enter Impact");
        //    else if (itm.Resolution == null)
        //        ModelState.AddModelError("Resolution", "Please enter Resolution");
        //    else if (itm.ResolvedDate == null)
        //        ModelState.AddModelError("Resolved", "Please enter Resolved");
        //    return ModelState.IsValid;
        //}



    }
}
