// Felipe Antonio Brüggemann 
using System.ComponentModel.DataAnnotations;

namespace AcademiaDaDuda.Application.Enums;

public enum AppColaboradorVinculo
{
    [Display(Name = "CLT")]
    CLT = 0,

    [Display(Name = "Estagiário")]
    Estagio = 1
}