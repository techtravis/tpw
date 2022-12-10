namespace TravisPWalker.Models
{
    public class HomePageEditViewModel
    {
        public HomePageEditViewModel()
        {
        }

        public HomePageEditViewModel(string html)
        {
            Html = html;
        }

        public string Html { get; set; } = "";
    }
}
