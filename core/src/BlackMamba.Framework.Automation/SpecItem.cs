using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Watcher.Core.Entity;
using Watcher.Core;

namespace BlackMamba.Framework.Automation
{
    public class SpecItem
    {
        
        public SpecItem()
        {
            this.Params = new List<Param>();
            this.Hosts = new List<SpecHost>();
            this.Results = new List<Result>();
            this.Headers = new List<Header>();
        }

        public SpecItem(string name)
            : base()
        {
            this.Name = name;
            this.Headers = new List<Header>();
            this.Params = new List<Param>();
            this.Hosts = new List<SpecHost>();
            this.Results = new List<Result>();
        }

        public SpecItem(string name, FixtureType type)
            : base()
        {
            this.Name = name;
            this.FixtureType = type;
            this.Headers = new List<Header>();
            this.Params = new List<Param>();
            this.Hosts = new List<SpecHost>();
            this.Results = new List<Result>();
        }

        public SpecItem(string name, HttpMethod method)
            : base()
        {
            this.Name = name;
            this.Method = method;
            this.Headers = new List<Header>();
            this.Params = new List<Param>();
            this.Hosts = new List<SpecHost>();
            this.Results = new List<Result>();
        }

        public SpecItem(string name, FixtureType type, HttpMethod method)
            : base()
        {
            this.Name = name;
            this.FixtureType = type;
            this.Method = method;
            this.Headers = new List<Header>();
            this.Params = new List<Param>();
            this.Hosts = new List<SpecHost>();
            this.Results = new List<Result>();
        }

        public string Name { get; set; }

        public FixtureType FixtureType { get; set; }

        public HttpMethod Method { get; set; }

        public List<SpecHost> Hosts { get; set; }

        public List<Result> Results { get; set; }

        public string LinkUrl
        {
            get
            {
                return _linkUrl;
            }
            set
            {
                if (value != null) value = value.Trim();

                var url = value;
                if (url.IndexOf("?") >= 0)
                {
                    _linkUrl = url.Substring(0, url.IndexOf("?"));

                    var parameters = url.Substring(url.IndexOf("?")).TrimStart('?').Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in parameters)
                    {
                        if (item.Contains("="))
                        {
                            var key = item.Split('=')[0];
                            var val = item.Split('=')[1];

                            AddParameter(key, val);
                        }
                    }
                }
                else
                {
                    _linkUrl = value;
                }
            }
        }private string _linkUrl;

        public List<Param> Params { get; set; }

        public List<Header> Headers { get; set; }

        #region Methods
        public void AddHost(string host)
        {
            var ix = host.Replace("http://", string.Empty).IndexOf(":");
            if (ix >= 0)
            {
                ix = host.LastIndexOf(":");
                var h = host.Substring(0, ix);
                var port = host.Substring(ix + 1).ToInt32();

                AddHost(h, port);
            }
            else
            {
                if (this.Hosts == null) this.Hosts = new List<SpecHost>();

                if (!this.Hosts.Exists(s => s.Name.EqualsOrdinalIgnoreCase(host)))
                {
                    this.Hosts.Add(new SpecHost { Name = host });
                }
            }
        }

        public void AddHost(string host, int port)
        {
            if (this.Hosts == null) this.Hosts = new List<SpecHost>();

            if (!this.Hosts.Exists(s => s.Name.EqualsOrdinalIgnoreCase(host) && s.Port == port))
            {
                this.Hosts.Add(new SpecHost { Name = host, Port = port });
            }
        }

        public void AddResult(string name, string expectValue, ResultType resultType, OP operation)
        {
            if (this.Results == null) this.Results = new List<Result>();

            if (!this.Results.Exists(s => s.Name.EqualsOrdinalIgnoreCase(name)))
            {
                var result = new Result { Name = name, Value = expectValue, DataType = resultType, Opertaion = operation };

                this.Results.Add(result);
            }
        }

        

        public void AddParameter(string name, string value)
        {
            AddParameter(name, value, ParamType.Value);
        }

        public void AddParameter(string name, string value, ParamType paramType)
        {
            if (this.Params == null) this.Params = new List<Param>();

            if (!this.Params.Exists(s => s.Name.EqualsOrdinalIgnoreCase(name)))
            {
                this.Params.Add(new Param { Name = name, Value = value, Type = paramType });
            }
        }

        public void AddPostBody(string body)
        {
            if (this.Method == HttpMethod.POST)
                AddParameter("post", body);
        }

        public void AddHeaders(string name, string value)
        {
            this.Headers.Add(new Header { Name=name,Value=value});
        }

        #endregion
    }
}
