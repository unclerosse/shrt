using ShRt.Models;
using ShRt.Data;

namespace ShRt.Services;

public static class LinkShortener
{  
    static readonly string chars = "02468BCDFGHJKLMNPQRSTVWXZbcdfghjklmnpqrstvwxz";

    private static string IdToLink(int id)
    {
        string result = "";

        while (id > 0)
        {
            int remainder = id % chars.Length;
            result = chars[remainder] + result;
            id /= chars.Length;
        }

        while (result.Length < 8)
            result = "0" + result;

        return result;
    }
    
    public static IResult PostLink(LinkDto link, LinksContext db, HttpContext ctx)
    {
        if (!Uri.TryCreate(link.Link, UriKind.Absolute, out var inputLink))
            return Results.BadRequest($"Invalid Link has been provided: {link.Link}");

        var lastId = db.Links.OrderByDescending(e => e.Id)
                .Select(e => e.Id)
                .FirstOrDefault();
        
        var shorted = new Link() 
        { 
            OriginalLink = link.Link,
            ShortedLink = IdToLink(lastId + 1)
        };

        db.Links.Add(shorted);
        db.SaveChangesAsync();
        
        var response = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{shorted.ShortedLink}";

        return Results.Ok(new LinkResonseDto()
        {
            Link = response
        });
    }

    public static IResult FallbackLink(LinksContext db, HttpContext ctx)
    {
        var path = ctx.Request.Path.ToUriComponent().Trim('/');

        var linkMatch = db.Links.FirstOrDefault(e => e.ShortedLink.Trim() == path.Trim());

        if (linkMatch == null)
            return Results.BadRequest("Invalid link has been found");

        return Results.Redirect(linkMatch.OriginalLink);    
    }
}