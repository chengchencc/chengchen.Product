using BlackMamba.Framework.Core;
using BlackMamba.Billing.Domain.ViewModels;
namespace BlackMamba.Billing.Domain.ViewModels
{
    public class Delimiters
    {
        public virtual string Property { get; set; }

        public virtual string Line { get; set; }

        public virtual string Third { get; set; }

        public virtual bool AppendDelimiterDefinitions { get; set; }

        public virtual bool? AppendPropertyNames { get; set; }

        public static Delimiters Default
        {
            get
            {
                return _defaultInstance;
            }
        }
        private static Delimiters _defaultInstance = new Delimiters
        {
            Line = ASCII.VERTICAL_BAR,
            Property = ASCII.SEMICOLON,
            Third = ASCII.COMMA,
            AppendDelimiterDefinitions = true,
            AppendPropertyNames = true
        };

        public static Delimiters Music { get { return _musicInstance; } }
        private static Delimiters _musicInstance = new Delimiters
        {
            Line = "|",
            Property = "<|>",
            Third = ",",
            AppendDelimiterDefinitions = false,
            AppendPropertyNames = false
        };

        public static Delimiters AVWiki { get { return _avWiki_Instance; } }
        private static Delimiters _avWiki_Instance = new Delimiters
        {
            Line = "|",
            Property = ",",
            Third = "~",
            AppendDelimiterDefinitions = false,
            AppendPropertyNames = false
        };

        public static Delimiters AVWikiDetail { get { return _avWiki_detail_Instance; } }
        private static Delimiters _avWiki_detail_Instance = new Delimiters
        {
            Line = "|",
            Property = "|",
            Third = "~",
            AppendDelimiterDefinitions = false,
            AppendPropertyNames = false
        };

        public override string ToString()
        {
            return string.Format("{0}{1}{2}", this.Line, this.Property, this.Third);
        }
    }
}
