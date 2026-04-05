namespace OrderManagement.Domain.Entities;

public class Buyer : BaseAuditableEntity
{
    public string Name { get; private set; } = default!;

    private Buyer() { }

    public Buyer(string name)
    {
        Name = name;
    }

    public void Update(string name)
    {
        Name = name;
    }
}