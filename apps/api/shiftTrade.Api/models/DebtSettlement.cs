namespace shiftTrade.api.models;


public class DebtSettlement
{
    public Guid Id {get;set;} = Guid.NewGuid();
    public Guid OrganizationId {get;set;}

     // The newly created reverse-direction debt.
    public Guid SourceDebtId {get;set;}

    public Guid TargetedDebtId   {get;set;}  // The older debt being reduced or settled.
     public decimal HoursApplied { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;


}