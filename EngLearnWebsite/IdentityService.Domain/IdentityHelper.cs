using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Domain;

public static class IdentityHelper
{
    /// <summary>
    /// 将IdentityResult中的错误信息格式化
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static string SumErrors(this IEnumerable<IdentityError> errors)
    {
        IEnumerable<string> strs = errors.Select(identityError => $"code = {identityError.Code}, message = {identityError.Description}");
        return String.Join("\n", strs);
    }
}
