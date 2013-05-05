using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace BlackMamba.Framework.Core
{
    public static class ObjectExtensions
    {
        public static Byte[] GetPostStream(this object stream)
        {
            var Request = HttpContext.Current.Request;
            Stream s = null;
            if (Request.Files.Count > 0)
                s = Request.Files[0].InputStream;
            else
                s = Request.InputStream;

            if (s == null) return null;

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, (int)s.Length);

            //SaveBinaryLog(bytes, "GetPostStream");

            return bytes;
        }
    }
}
