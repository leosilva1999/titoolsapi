﻿namespace TiTools_backend.DTOs
{
    public class EquipmentUpdateDTO
    {
        public string? EquipmentName { get; set; }
        public string? IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public bool? EquipmentLoanStatus { get; set; }
    }
}
