using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CkasTool_MVVM.ViewModels
{
    public class ErrorViewModel : BaseViewModel
    {
        private string _errorData;
        public string ErrorData { get { return _errorData; } set { _errorData = value; OnPropertyChanged(); } }

        private string _title = "Error";
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(); } }

    }
}
