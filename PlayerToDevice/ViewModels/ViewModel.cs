using System.ComponentModel;
using System.Runtime.CompilerServices;
using PlayerToDevice.Annotations;

namespace PlayerToDevice.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool ChangeField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            var result = false;
            if ((field != null && !field.Equals(value)) || (field == null && value != null))
            {
                field = value;
                OnPropertyChanged(propertyName);
                result = true;
            }
            return result;
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}