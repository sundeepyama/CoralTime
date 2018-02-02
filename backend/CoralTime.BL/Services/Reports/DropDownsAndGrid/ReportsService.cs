﻿using AutoMapper;
using CoralTime.BL.Interfaces.Reports;
using CoralTime.DAL.Repositories;
using System;

namespace CoralTime.BL.Services.Reports.DropDownsAndGrid
{
    public partial class ReportsService : BaseService, IReportsService
    {
        public ReportsService(UnitOfWork uow, IMapper mapper)
            : base(uow, mapper) { }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}