﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GradDisplayMain.Models
{
    public class AdministratorRequirement : AuthorizationHandler<AdministratorRequirement>, IAuthorizationRequirement
    {

        public static string ConnectionString { get; set; }
        
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdministratorRequirement requirement)
        {

             
            IList<string> roles = new List<string>();

          
            if (!String.IsNullOrEmpty(ConnectionString))
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                   using (SqlCommand cmd = new SqlCommand("SELECT Name FROM AspNetRoles WHERE Name='Administrator'", con))
                    {
                        cmd.CommandType = CommandType.Text;

                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                roles.Add(rdr["Name"].ToString());
                            }
                        }

                        cmd.Dispose();
                    }
                }


                var userIsInRole = roles.Any(role => context.User.IsInRole(role));
                if (!userIsInRole)
                {
                    // context.Fail();
                    // return null;
                    var mvcContext = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;

                    if (mvcContext != null)
                    {
                        mvcContext.HttpContext.Response.Redirect("/Account/Login");
                    }

                }  
            }

            if (!context.User.HasClaim(claim => claim.Value == "Administrator"))
            {
                // handled above
                // redirect to login 
                // context.Fail();
            } else
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}
