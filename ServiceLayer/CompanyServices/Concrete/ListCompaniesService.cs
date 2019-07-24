// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using Microsoft.AspNetCore.Html;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.CompanyServices.Concrete
{
    /// <summary>
    /// This displays a list of the Company hierarchies in a simple way
    /// ONLY USED FOR DEMO
    /// </summary>
    public class ListCompaniesService : IListCompaniesService
    {
        private readonly CompanyDbContext _context;

        public ListCompaniesService(CompanyDbContext context)
        {
            _context = context;
        }

        public List<HtmlString> BuildViewOfHierarchy()
        {
            var result = new List<HtmlString>();
            foreach (var company in _context.Tenants.IgnoreQueryFilters().OfType<Company>()
                .Include(x => x.Children)
                .ThenInclude(x => x.Children)
                .ThenInclude(x => x.Children)
                .ThenInclude(x => x.Children)
                .ThenInclude(x => x.Children))
            {
                var sb = new StringBuilder("<table class=\"table\">");
                sb.Append(HtmlDisplayTenant(company, 0));
                ShowChildren(company.Children, sb, 1);
                sb.Append("</table>");
                result.Add(new HtmlString(sb.ToString()));
            }

            return result;
        }

        private void ShowChildren(IEnumerable<TenantBase> children, StringBuilder sb, int indent)
        {            
            foreach (var tenant in children)
            {
                sb.Append(HtmlDisplayTenant(tenant, indent));
                if (tenant.Children.Any())
                    ShowChildren(tenant.Children, sb, indent+1);
            }
        }

        private string HtmlDisplayTenant(TenantBase tenant, int indent)
        {
            var result = "<tr>";
            for (int i = 0; i < indent; i++)
            {
                result += "<td></td>";
            }
            result +=       $"<td>{tenant.GetType().Name}</td>" +
                   $"<td>{tenant.Name}</td>" +
                   $"<td>DataKey = {tenant.DataKey}</td>" +
                   "</tr>";

            return result;
        }
    }
}