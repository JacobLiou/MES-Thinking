using MES.Domain.Entities;

namespace MES.Domain.Services;

public static class TestFlowExtensions
{
    public static RouteStep? GetNextStep(this TestFlow flow, int? completedStepSequence)
    {
        var ordered = flow.Steps.OrderBy(s => s.Sequence).ToList();
        if (ordered.Count == 0)
        {
            return null;
        }

        if (completedStepSequence is null)
        {
            return ordered[0];
        }

        return ordered.FirstOrDefault(s => s.Sequence > completedStepSequence.Value);
    }

    public static RouteStep? GetStepBySequence(this TestFlow flow, int sequence) =>
        flow.Steps.FirstOrDefault(s => s.Sequence == sequence);

    public static RouteStep? GetLastStep(this TestFlow flow) =>
        flow.Steps.OrderByDescending(s => s.Sequence).FirstOrDefault();
}
