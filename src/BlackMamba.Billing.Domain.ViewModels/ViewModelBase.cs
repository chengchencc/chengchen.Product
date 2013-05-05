namespace BlackMamba.Billing.Domain.ViewModels
{
    public abstract class ViewModelBase : IViewModel
    {
        public virtual string ToViewModelString()
        {
            return this.ToString();
        }
    }
}