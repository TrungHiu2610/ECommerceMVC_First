using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;

namespace ECommerceMVC.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterVM, KhachHang>();
        }
    }
}
