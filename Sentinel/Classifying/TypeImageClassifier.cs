#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

#region Using directives

using Sentinel.Images;
using Sentinel.Services;

#endregion

namespace Sentinel.Classifying
{

    #region Using directives

    #endregion

    public class TypeImageClassifier : VisualClassifier
    {
        private string image;

        public TypeImageClassifier(string type, string image)
            : base("Type Image Classifier")
        {
            TypeMatch = type;
            Image = image;
        }

        public string Image
        {
            get
            {
                return image;
            }

            set
            {
                if (value != image)
                {
                    image = value;

                    // Register self to ImageService
                    ITypeImageService service = ServiceLocator.Instance.Get<ITypeImageService>();
                    if (service != null)
                    {
                        service.Register(TypeMatch, image);
                    }
                }
            }
        }

        public string TypeMatch { get; private set; }

        public override bool IsMatch(object parameter)
        {
            return parameter != null && ((parameter is string) && (parameter as string).Equals(TypeMatch));
        }
    }
}