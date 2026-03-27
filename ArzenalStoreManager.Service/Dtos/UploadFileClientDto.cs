using Arzenal.Dto.DTOs.FileDto;

namespace Arzenal.StoreManager.Core.Dtos
{
    public class UploadFileClientDto
    {
        public UploadFileDto ApiDto { get; set; }  // le DTO partagé avec l'API
        public string LocalPath { get; set; }      // chemin complet local
    }

}
