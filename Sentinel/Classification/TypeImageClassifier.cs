#region License
//
// © Copyright Ray Hayes
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
//
#endregion

namespace Sentinel.Classification
{
    using Sentinel.Images;
    using Sentinel.Images.Interfaces;
    using Sentinel.Interfaces;
    using Sentinel.Services;

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
                    var service = ServiceLocator.Instance.Get<ITypeImageService>();
                    if (service != null)
                    {
                        // TODO: what quality?  defaulting to small.
                        service.Register(TypeMatch, ImageQuality.Small, image);
                    }
                }
            }
        }

        public string TypeMatch { get; private set; }

        public override bool IsMatch(object parameter)
        {
            return (parameter is string) && (parameter as string).Equals(TypeMatch);
        }

        public override LogEntry Classify(LogEntry entry)
        {
            // TODO: something?
            return entry;
        }
    }
}