using System.ComponentModel.Composition;
using Sentinel.Images.Interfaces;

namespace Sentinel.Images
{
    [Export(typeof(IRemoveTypeImage))]
    public class RemoveTypeImageMapping : IRemoveTypeImage
    {
    }
}