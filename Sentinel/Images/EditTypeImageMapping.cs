using System.ComponentModel.Composition;
using Sentinel.Images.Interfaces;

namespace Sentinel.Images
{
    [Export(typeof(IEditTypeImage))]
    public class EditTypeImageMapping : IEditTypeImage
    {
    }
}