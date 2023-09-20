using ELearning.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ELearning.Controllers
{
    public class HomeController : Controller
    {
        private ElearningDataClassesDataContext db = new ElearningDataClassesDataContext();
        private bool connect = false;
        public ActionResult Index()
        {

            var listcours = (from el in db.cours
                              orderby el.views descending
                              select el).Take(3);

            try
            {
                if (Session["id"].ToString() != null)
                    ViewBag.user = Session["id"].ToString().Trim();
                ViewBag.image = "../ProfileImages/" + Session["id"].ToString().Trim() + ".jpg";


            }
            catch (Exception e)
            {
                ViewBag.user = "Guest";
                ViewBag.image = "https://cdn1.iconfinder.com/data/icons/flat-character-color-1/60/flat-design-character_6-512.png";
            }

            return View(listcours); 
        }

        public ActionResult SignIn()
        {
            try
            {
                if (Session["id"].ToString() != null)
                    return RedirectToAction("ListCours");
            }
            catch (Exception ex)
            {

            }

            return View();
        }

        [HttpPost]
        public ActionResult SignIn(user e)
        {
            if (!ModelState.IsValidField("Id") || !ModelState.IsValidField("password"))
                return View();
            try
            {
                user e1 = (from el in db.users
                               where el.Id == e.Id && el.password == e.password
                               select el).Single<user>();

                Session["id"] = e1.Id;
                Session["type"] = e1.typeuser;
                Session["actif"] = e1.actif;
                Session["level"] = e1.level;
                connect = true;
                if (e1.actif == 0)
                    return View("ActivateAccount");
                else
                    return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                if (ex is InvalidOperationException)
                    ModelState.AddModelError("password", "UserName or/and Password Incorrect");
                else
                    ModelState.AddModelError("password", "Something Went Wrong");

                return View();
            }
        }

        public ActionResult SignUp()
        {
            try
            {
                if (Session["id"].ToString() != null)
                    return RedirectToAction("liste");

            }
            catch (Exception ex)
            {

            }

            return View();
        }

        [HttpPost]
        public ActionResult SignUp(user e)
        {

            if (!ModelState.IsValid)
                return View();

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringCharsuser = new char[4];
            var stringCharspass = new char[6];
            var random = new Random();

            for (int i = 0; i < stringCharsuser.Length; i++)
            {
                stringCharsuser[i] = chars[random.Next(chars.Length)];
            }
            for (int i = 0; i < stringCharspass.Length; i++)
            {
                stringCharspass[i] = chars[random.Next(chars.Length)];
            }


            e.Id = new String(stringCharsuser);
            e.password = new String(stringCharspass);
            e.actif = 0;
            e.typeuser = "student";


            db.users.InsertOnSubmit(e);
            db.SubmitChanges();
            string a = Email.sendMail(new Email { To = e.mail, Subject = "Here your first time login activation ! ", Message = "Temporary username: " + e.Id + "\n Temporary password: " + e.password });
            ViewBag.msn = a;


            ViewBag.accept = "Registered successfully ! We sent an E-Mail for ur activation infos...";
            return View();
        }

        public ActionResult ActivateAccount()
        {
            try
            {
                if (Session["actif"].ToString() == "1")
                    return RedirectToAction("ListCours");
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("index");
            }
        }

        [HttpPost]
        public ActionResult ActivateAccount(user e)
        {

            if (!ModelState.IsValidField("Id") || !ModelState.IsValidField("password"))
                return View();

            try
            {
                user e1 = (from el in db.users
                               where el.Id == Session["id"].ToString()
                               select el).Single<user>();
                e.actif = 1;
                e.level = e1.level;
                e.nom = e1.nom;
                e.prenom = e1.prenom;
                e.mail = e1.mail;
                e.typeuser = "student";

                db.users.InsertOnSubmit(e);

                db.users.DeleteOnSubmit(e1);
                db.SubmitChanges();

                Session["id"] = e1.Id;
                Session["type"] = "student";
                Session["actif"] = e1.actif;
                Session["level"] = e1.level;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Id", "Alrady Exists");
                return View();
            }

            return Logout();
        }
        public ActionResult profile(string id)
        {
            try
            {
                if (Session["id"].ToString().Trim() == id)
                {
                    ViewBag.pic = "<span>Change Photo</span><input onchange = 'this.form.submit()' type = 'file' name = 'file'/> ";
                    ViewBag.manc = "<button type='button' class='close' data-dismiss='modal' aria-hidden='true'>×</button>";
                }
                else
                {
                    ViewBag.pic = "";
                    ViewBag.manc = "";
                }

                user e1 = (from el in db.users where el.Id == id select el).Single<user>();

                if (Session["type"].ToString().Trim() == "student")
                {
                    var liste = (from el in db.inscriptions join ell in db.cours on el.cours equals ell.Id where el.user == id select ell);
                    ViewBag.inscrit = liste.Count();
                    ViewBag.cours = liste;
                }
                else
                {
                    var liste = (from ell in db.cours where ell.author == id select ell);
                    ViewBag.inscrit = liste.Count();
                    ViewBag.cours = liste;
                }

                if (e1.level == 1)
                    ViewBag.level = "Secondaire";
                else if (e1.level == 2)
                    ViewBag.level = "Collegial";
                else if (e1.level == 3)
                    ViewBag.level = "Universitaire";
                else
                    ViewBag.level = "Teacher";

                ViewBag.tout = (from el in db.cours where el.level == e1.level.ToString() select el).Count();

                return View(e1);
            }
            catch (Exception e)
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult AddCours()
        {
            try
            {
                if (Session["type"].ToString() == "teacher")
                    return View();
            }
            catch (Exception e)
            {
                return RedirectToAction("SignIn");
            }
            return RedirectToAction("SignIn");

        }
        [HttpPost]
        public ActionResult AddCours(cour c, HttpPostedFileBase file)
        {
            c.views = 0;
            c.author = Session["id"].ToString().Trim();

            db.cours.InsertOnSubmit(c);
            db.SubmitChanges();

            var path = Path.Combine(Server.MapPath("~/CoursImages"), c.Id + ".jpg");
            file.SaveAs(path);

            return RedirectToAction("cours");
        }
        public ActionResult ListCours()
        {


            try
            {
                if (Session["type"].ToString().Trim() == "student")
                {
                    var listcours = (from el in db.cours where el.level == Session["level"].ToString() select el);
                    foreach (cour e in listcours)
                    {
                        try
                        {
                            int l = (from el in db.inscriptions
                                     where el.cours == e.Id && el.user == Session["id"].ToString()
                                     select el.cours).Single<int>();
                            e.status = 1;

                        }
                        catch (System.InvalidOperationException ex)
                        {

                            e.status = 0;
                        }
                    }
                    ViewBag.listcours = listcours;

                }
                else
                {
                    var listcours = (from el in db.cours where el.author == Session["id"].ToString() select el);
                    ViewBag.listcours = listcours;
                }

                return View();
            }
            catch (Exception e)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public JsonResult inscrire()
        {
            inscription i = new inscription();

            i.cours = int.Parse(Request["cours"].ToString());
            i.user = Request["password"].ToString();

            try
            {
               user e1 = (from el in db.users
                               where el.Id == Session["id"].ToString() && el.password == i.user
                               select el).Single<user>();

                cour cc = (from el in db.cours where el.Id == i.cours select el).Single<cour>();
                inscription inscri = new inscription();
                inscri.cours = i.cours;
                inscri.user = Session["id"].ToString();

                db.inscriptions.InsertOnSubmit(inscri);
                db.SubmitChanges();
            }
            catch (System.InvalidOperationException ex)
            {
                return Json("false", JsonRequestBehavior.AllowGet);
            }

            return Json("true", JsonRequestBehavior.AllowGet);
        }


        public ActionResult Cours(int? id)
        {
            cour cours;
            try
            {


                if (Session["type"].ToString().Trim() == "student")
                {
                    (from el in db.inscriptions where el.cours == id && el.user == Session["id"].ToString() select el.cours).Single<int>();

                    cours = (from el in db.cours where el.Id == id select el).Single<cour>();

                    if (cours.level == "1")
                        ViewBag.level = "Secondaire";
                    else if (cours.level == "2")
                        ViewBag.level = "Collegial";
                    else
                        ViewBag.level = "Universitaire";

                    cours.views++;
                    db.SubmitChanges();
                }
                else
                {
                    cours = (from el in db.cours where el.Id == id select el).Single<cour>();
                    if (cours.level == "1")
                        ViewBag.level = "Secondaire";
                    else if (cours.level == "2")
                        ViewBag.level = "Collegial";
                    else
                        ViewBag.level = "Universitaire";

                    cours.views++;
                    db.SubmitChanges();
                }

            }


            catch (Exception ex)
            {

                return RedirectToAction("Index");
            }

            try
            {
            }
            catch (System.InvalidOperationException ex)
            {


            }
            return View(cours);
        }


        [HttpPost]
        public JsonResult DropCours()
        {

            if (Session["type"].ToString() == "student")
            {
                inscription i = new inscription();
                i.cours = int.Parse(Request["cours"].ToString());
                i.user = Session["id"].ToString().Trim();

                db.inscriptions.Attach(i);
                db.inscriptions.DeleteOnSubmit(i);
                db.SubmitChanges();
                return Json(i.cours + i.user, JsonRequestBehavior.AllowGet);
            }
            else
            {
                cour c = (from el in db.cours where el.Id == int.Parse(Request["cours"].ToString()) select el).Single<cour>();

                db.cours.DeleteOnSubmit(c);
                db.SubmitChanges();
                return Json(c.Id + Session["id"].ToString(), JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Logout()
        {
            Session["id"] = null;
            Session["type"] = null;
            Session["actif"] = null;
            Session["level"] = null;
            connect = false;
            Session.Abandon();

            return View("SignIn");
        }
        [HttpPost]
        public ActionResult ProcessFile(HttpPostedFileBase file)
        {



            var path = Path.Combine(Server.MapPath("~/ProfileImages"), Session["id"].ToString().Trim() + ".jpg");
            file.SaveAs(path);



            return RedirectToAction("profile", "Home", new { @id = Session["id"].ToString().Trim() });
        }
    }
}