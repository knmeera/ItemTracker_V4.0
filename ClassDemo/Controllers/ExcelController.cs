using ERP.Project.Data.Context;
using ERP.Project.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

namespace ClassDemo.Controllers
{
    [IsAdmin]
    public class ExcelController : BaseController
    {
        string connect = WebConfigurationManager.ConnectionStrings["MSSqlCon"].ToString();
        ProjectManagementDbContext db = new ProjectManagementDbContext();
        public ActionResult Excel(int? page)
        {
            var res = db.TrackerItems.ToList().OrderByDescending(m => m.ItemId).ToPagedList(page ?? 1, 9);
            return View(res);
        }

        //excel file to datadase table trackeritem
        [HttpPost]
        public ActionResult Excel(HttpPostedFileBase file)
        {
            if (file != null)
            {
                try
                {
                    string con = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1;\"", file.FileName);
                    OleDbConnection Econ = new OleDbConnection(con);

                    string Query = string.Format("Select * from [{0}]", "Sheet1$");
                    //OleDbCommand com = new OleDbCommand(Query, Econ);

                    Econ.Open();
                    TrackerItem tracker = new TrackerItem();
                    OleDbDataAdapter Da = new OleDbDataAdapter(Query, Econ);
                    DataTable Dt = new DataTable();

                    Da.Fill(Dt);
                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (DataRow dr in Dt.Rows)
                            {

                                tracker.ItemSummary = dr["ItemSummary"].ToString();
                                tracker.ItemCreatedDate = dr["ItemCreatedDate"].ToString();
                                tracker.AssignedDate = dr["AssignedDate"].ToString();
                                tracker.ItemEndDate = dr["ItemEndDate"].ToString();
                                tracker.WorkCompleted = dr["WorkCompleted"].ToString();
                                tracker.CreatedBy = dr["CreatedBy"].ToString();
                                tracker.Owner = dr["Owner"].ToString();
                                tracker.Impact = dr["Impact"].ToString();
                                tracker.Resolution = dr["Resolution"].ToString();
                                tracker.ResolvedDate = dr["ResolvedDate"].ToString();
                                tracker.AttachmentPath = dr["AttachmentPath"].ToString();
                                tracker.ParentId = Convert.ToInt32(dr["ParentId"]);
                                tracker.AssignedTo = dr["AssignedTo"].ToString();
                                tracker.Subject = dr["Subject"].ToString();
                                tracker.ItemCategoryId = Convert.ToInt32(dr["ItemCategoryId"]);
                                tracker.ItemProjectId = Convert.ToInt32(dr["ItemProjectId"]);
                                tracker.ItemTypeId = Convert.ToInt32(dr["ItemTypeId"]);
                                tracker.ItemPriorityId = Convert.ToInt32(dr["ItemPriorityId"]);
                                tracker.ItemStatusId = Convert.ToInt32(dr["ItemStatusId"]);

                                db.TrackerItems.Add(tracker);
                                db.SaveChanges();
                            }
                            dbTran.Commit();
                        }
                        catch (Exception ex)
                        {
                            ViewBag.Error = ex.Message.ToString();
                            dbTran.Rollback();
                        }

                        Econ.Close();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message.ToString();
                }
            }
            ViewBag.Error = "file not selected properly";
            return RedirectToAction("Excel");
        }
        public FileContentResult GetExcel()
        {
            string filePath = Path.Combine(Server.MapPath(@"~/Content\Excel\ExportFile.xlsx"));
            string con = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1;\"", filePath);

            
            DataTable dt = new DataTable();
            string query = "Insert into [Sheet1$] (ItemId,ItemSummary,ItemCreatedDate,AssignedDate,ItemEndDate,WorkCompleted,CreatedBy,Owner,Impact,Resolution,ResolvedDate,AttachmentPath,ParentId,AssignedTo,Subject,ItemProjectId,ItemCategoryId,ItemTypeId,ItemPriorityId,ItemStatusId)" +
                " values(@ItemId,@ItemSummary,@ItemCreatedDate,@AssignedDate,@ItemEndDate,@WorkCompleted,@CreatedBy,@Owner,@Impact,@Resolution,@ResolvedDate,@AttachmentPath,@ParentId,@AssignedTo,@Subject,@ItemProjectId,@ItemCategoryId,@ItemTypeId,@ItemPriorityId,@ItemStatusId)";
            OleDbConnection conn = new OleDbConnection(con);
            conn.Open();
            OleDbCommand com = new OleDbCommand(query,conn);
            if (TempData["items"]==null)
            {
                SqlConnection cone = new SqlConnection(connect);
                SqlDataAdapter da = new SqlDataAdapter("select * from trackeritem",cone);
                da.Fill(dt);
            }
            if(TempData["items"] != null)
            {
               
            }
            foreach (DataRow dr in dt.Rows)
            {
                com.Parameters.AddWithValue("@ItemId", Convert.ToInt32(dr["ItemId"]));
                com.Parameters.AddWithValue("@ItemSummary", dr["ItemSummary"].ToString());
                com.Parameters.AddWithValue("@ItemCreatedDate", dr["ItemCreatedDate"].ToString());
                com.Parameters.AddWithValue("@AssignedDate", dr["AssignedDate"].ToString());
                com.Parameters.AddWithValue("@ItemEndDate", dr["WorkCompleted"].ToString());
                com.Parameters.AddWithValue("@WorkCompleted", dr["WorkCompleted"].ToString());
                com.Parameters.AddWithValue("@CreatedBy", dr["CreatedBy"].ToString());
                com.Parameters.AddWithValue("@Owner", dr["Owner"].ToString());
                com.Parameters.AddWithValue("@Impact", dr["Impact"].ToString());
                com.Parameters.AddWithValue("@Resolution", dr["Resolution"].ToString());
                com.Parameters.AddWithValue("@ResolvedDate", dr["ResolvedDate"].ToString());
                com.Parameters.AddWithValue("@AttachmentPath", dr["AttachmentPath"].ToString());
                com.Parameters.AddWithValue("@ParentId", Convert.ToInt32(dr["ItemStatusId"]));
                com.Parameters.AddWithValue("@AssignedTo", dr["AssignedTo"].ToString());
                com.Parameters.AddWithValue("@Subject", dr["Subject"].ToString());
                com.Parameters.AddWithValue("@ItemProjectId", Convert.ToInt32(dr["ItemProjectId"]));
                com.Parameters.AddWithValue("@ItemCategoryId", Convert.ToInt32(dr["ItemCategoryId"]));
                com.Parameters.AddWithValue("@ItemTypeId", Convert.ToInt32(dr["ItemTypeId"]));
                com.Parameters.AddWithValue("@ItemPriorityId", Convert.ToInt32(dr["ItemPriorityId"]));
                com.Parameters.AddWithValue("@ItemStatusId", Convert.ToInt32(dr["ItemStatusId"]));
            }
            com.ExecuteNonQueryAsync();
            conn.Close();

            var mimeType = "application/vnd.ms-excel";
            var fileContents = System.IO.File.ReadAllBytes(filePath);
            return new FileContentResult(fileContents, mimeType) { FileDownloadName = Path.GetFileName(filePath) };
        }
        public FileContentResult DownloadSampleFile()
        {
            string filePath = string.Empty;

                filePath = Path.Combine(Server.MapPath(@"~/Content\Excel\DownloadSample.xlsx"));

            var mimeType = "application/vnd.ms-excel";
            var fileContents = System.IO.File.ReadAllBytes(filePath);
            return new FileContentResult(fileContents, mimeType) { FileDownloadName = Path.GetFileName(filePath) };
        }
        public ActionResult Edit(int ID, string Name)
        {
            if (Session["UserName"].ToString() == Name || Session["UserName"].ToString() == "admin")
            {
                ViewBag.Project = new SelectList(db.projects, "ProjectId", "ProjectName");
                ViewBag.CategoryVB = new SelectList(db.ItemCategories, "CategoryId", "CategoryName");
                ViewBag.ItemTypeVB = new SelectList(db.ItemTypes, "ItemTypeId", "ItemName");
                ViewBag.ItemStatusVB = new SelectList(db.ItemStatuses, "ItemStatusId", "ItemStatusName");
                ViewBag.ItemPriorityVB = new SelectList(db.ItemPriorities, "PriorityId", "PriorityName");


                //TrackerItem itm = db.TrackerItems.Find(ID);
                TrackerItem item = db.TrackerItems.Include(m => m.ItemCategory).Include(m => m.ItemPriority).Include(m => m.ItemStatus).Include(m => m.project).Include(m => m.ItemType).SingleOrDefault(x => x.ItemId == ID);
                return View(item);
            }
            else
            {
                return RedirectToAction("Excel");
            }
        }
        [HttpPost]
        public ActionResult Edit(TrackerItem itm)
        {

            db.Entry(itm).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Excel");
        }
        public ActionResult DetailsView(int Id, string Name)
        {
            if (Session["UserName"].ToString() == Name || Session["UserName"].ToString() == "admin")
            {
                TrackerItem itm = db.TrackerItems.Find(Id);

                return View(itm);
            }
            else
            {
                return RedirectToAction("Excel");
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
            return RedirectToAction("Excel");
        }
        
    }
}