using Arzenal.Dto.DTOs.AppDto;
using Arzenal.StoreManager.Core.Models;
using Riok.Mapperly.Abstractions;

namespace Arzenal.StoreManager.Core.Mapping
{
    [Mapper]
    public partial class AppMapper
    {
        public partial AppModel MapDtoToModel(ReadAppDto dto);

        public partial ReadAppDto MapModelToDto(AppModel model);

    }
}
