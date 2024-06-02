﻿using AutoMapper;
using AutoMapper.Internal;
using Northwind.ViewModels;
using Northwind.EntityModels;
using System.ComponentModel;
namespace MappingObjects.Mappers;

public static class CartToSummaryMapper
{
    public static MapperConfiguration GetMapperConfiguration(){
        MapperConfiguration config = new(cfg => {
            cfg.Internal().MethodMappingEnabled = false; // to fix an issue with the MaxInteger method
            cfg.CreateMap<Cart, Summary>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    string.Format("{0} {1}", src.customer.FirstName, src.customer.LastName)))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(
                    src => src.Items.Sum(item => item.UnitPrice * item.Quantity)));
        });
        return config;
    }
}
