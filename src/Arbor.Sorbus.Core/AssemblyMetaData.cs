namespace Arbor.Sorbus.Core
{
    public class AssemblyMetaData
    {
        readonly string _company;
        readonly string _configuration;
        readonly string _copyright;
        readonly string _description;
        readonly string _product;
        readonly string _trademark;

        public AssemblyMetaData(string description = null,
            string configuration = null,
            string company = null,
            string product = null,
            string copyright = null,
            string trademark = null)
        {
            _description = description;
            _configuration = configuration;
            _company = company;
            _product = product;
            _copyright = copyright;
            _trademark = trademark;
        }

        public string Description
        {
            get { return _description; }
        }

        public string Configuration
        {
            get { return _configuration; }
        }

        public string Company
        {
            get { return _company; }
        }

        public string Product
        {
            get { return _product; }
        }

        public string Copyright
        {
            get { return _copyright; }
        }

        public string Trademark
        {
            get { return _trademark; }
        }
    }
}