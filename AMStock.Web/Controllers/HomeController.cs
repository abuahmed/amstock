using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Service;
using WebMatrix.WebData;

namespace AMStock.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public HomeController()
        {
            Singleton.Edition = AMStockEdition.ServerEdition;
            Singleton.SqlceFileName = "AMStockDb2"; //"AmStockDbWeb1";//
            Singleton.SeedDefaults = true;

            InitializeWebSecurity();
            //Singleton.Edition = AMStockEdition.CompactEdition;
            //var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PinnaStock\\";
            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);
            //var pathfile = Path.Combine(path, "AMStockDbAbduMu.sdf");//AMStockDbHalal
            //Singleton.SqlceFileName = pathfile;
            //Singleton.SeedDefaults = true;

            Singleton.User = new UserService(true).GetAll().FirstOrDefault();
                
            //Singleton.UserRoles = new UserRolesModel();

            #region Warehouse Filter
            var warehouseList = new WarehouseService(true).GetWarehousesPrevilegedToUser(1).ToList();

            if (warehouseList.Count > 1)
                warehouseList.Insert(warehouseList.Count, new WarehouseDTO
                {
                    DisplayName = "All",
                    Id = -1
                });

            Singleton.WarehousesList = warehouseList;
            //ViewData["WarehouseList"] = warehouseList;

          
            #endregion
            
            //Singleton.Edition = AMStockEdition.CompactEdition;
            //var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\AMStock\\";
            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);
            //var pathfile = Path.Combine(path, "AMStockDbMedina.sdf");//AMStockDbMedinaLatest
            //Singleton.SqlceFileName = pathfile;
            //Singleton.SeedDefaults = true;
        }
        
        public ActionResult Index()
        {
            return View();
        }

        private void InitializeWebSecurity()
        {
            var dbContext2 = DbContextUtil.GetDbContextInstance();
            try
            {
                if (!WebSecurity.Initialized)
                    WebSecurity.InitializeDatabaseConnection(Singleton.ConnectionStringName, Singleton.ProviderName, "Users",
                        "UserId", "UserName", autoCreateTables: false);

                //var allRoles2 = Roles.GetAllRoles();

                if (!new UserService(true).GetAll().Any())
                {
                    #region Seed Default Roles and Users

                    IList<RoleDTO> listOfRoles = CommonUtility.GetRolesList();

                    foreach (var role in listOfRoles)
                    {
                        dbContext2.Set<RoleDTO>().Add(role);
                    }
                    dbContext2.SaveChanges();

                    WebSecurity.CreateUserAndAccount("superadmin", "P@ssw0rd1!",
                        new
                        {
                            Status = 1,
                            Enabled = true,
                            RowGuid = Guid.NewGuid(),
                            Email = "superadmin@amihanit.com",
                            CreatedByUserId = 1,
                            DateRecordCreated = DateTime.Now,
                            ModifiedByUserId = 1,
                            DateLastModified = DateTime.Now
                        });
                    WebSecurity.CreateUserAndAccount("adminuser", "P@ssw0rd",
                        new
                        {
                            Status = 0,
                            Enabled = true,
                            RowGuid = Guid.NewGuid(),
                            Email = "adminuser@amihanit.com",
                            CreatedByUserId = 1,
                            DateRecordCreated = DateTime.Now,
                            ModifiedByUserId = 1,
                            DateLastModified = DateTime.Now
                        });
                    WebSecurity.CreateUserAndAccount("amstock", "pa12345",
                        new
                        {
                            Status = 0,
                            Enabled = true,
                            RowGuid = Guid.NewGuid(),
                            Email = "amstock@amihanit.com",
                            CreatedByUserId = 1,
                            DateRecordCreated = DateTime.Now,
                            ModifiedByUserId = 1,
                            DateLastModified = DateTime.Now
                        });
                    //add row guid for membership table members
                    var members = new UserService().GetAllMemberShips();
                    foreach (var membershipDTO in members)
                    {
                        membershipDTO.RowGuid = Guid.NewGuid();
                        membershipDTO.Enabled = true;
                        membershipDTO.CreatedByUserId = 1;
                        membershipDTO.DateRecordCreated = DateTime.Now;
                        membershipDTO.ModifiedByUserId = 1;
                        membershipDTO.DateLastModified = DateTime.Now;
                        dbContext2.Set<MembershipDTO>().Add(membershipDTO);
                        dbContext2.Entry(membershipDTO).State = EntityState.Modified;
                    }
                    dbContext2.SaveChanges();

                    var lofRoles = new UserService().GetAllRoles().ToList();
                    foreach (var role in lofRoles)
                    {
                        dbContext2.Set<UsersInRoles>().Add(new UsersInRoles
                        {
                            RoleId = role.RoleId,
                            UserId = WebSecurity.GetUserId("superadmin")
                        });
                    }
                    foreach (var role in lofRoles.Skip(5))
                    {
                        dbContext2.Set<UsersInRoles>().Add(new UsersInRoles
                        {
                            RoleId = role.RoleId,
                            UserId = WebSecurity.GetUserId("adminuser")
                        });
                    }
                    foreach (var role in lofRoles.Skip(11))
                    {
                        dbContext2.Set<UsersInRoles>().Add(new UsersInRoles
                        {
                            RoleId = role.RoleId,
                            UserId = WebSecurity.GetUserId("amstock")
                        });
                    }
                    dbContext2.SaveChanges();
                    //var allRoles = Roles.GetAllRoles();
                    //Roles.AddUserToRoles("superadmin", allRoles);
                    //var rol1 = allRoles.Skip(5).ToArray();
                    //Roles.AddUserToRoles("adminuser", rol1);
                    //var rol2 = allRoles.Skip(11).ToArray();
                    //Roles.AddUserToRoles("amstock", rol2);

                    ////add row guid for UsersInRoles table members
                    //var userinroles = new UserService().GetAllUsersInRoles();
                    //foreach (var usersInRolesDTO in userinroles)
                    //{
                    //    usersInRolesDTO.RowGuid = Guid.NewGuid();
                    //    usersInRolesDTO.Enabled = true;
                    //    usersInRolesDTO.CreatedByUserId = 1;
                    //    usersInRolesDTO.DateRecordCreated = DateTime.Now;
                    //    usersInRolesDTO.ModifiedByUserId = 1;
                    //    usersInRolesDTO.DateLastModified = DateTime.Now;
                    //    dbContext2.Set<UsersInRoles>().Add(usersInRolesDTO);
                    //    dbContext2.Entry(usersInRolesDTO).State = EntityState.Modified;
                    //}
                    //dbContext2.SaveChanges();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Problem on InitializeWebSecurity" +
                //    Environment.NewLine + ex.Message +
                //    Environment.NewLine + ex.InnerException);
            }
            finally
            {
                dbContext2.Dispose();
            }
        }
        
    }
}
