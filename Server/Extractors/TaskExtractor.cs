using Server.Interfaces;
using System.Linq.Expressions;

namespace Server.Extractors
{
    public class TaskExtractor : IExtractor<Task>
    {
        private readonly Dictionary<Type, Func<Task, object>> _extractors = new();

        public object? ExtractValue(Task input)
        {
            Type taskType = input.GetType();
            if (!taskType.IsGenericType)
            {
                return null;
            }

            if (!_extractors.TryGetValue(taskType, out Func<Task, object>? val))
            {
                _extractors.Add(taskType, val = CreateExtractor(taskType));
            }

            return val(input);
        }

        private Func<Task, object> CreateExtractor(Type taskType)
        {
            ParameterExpression param = Expression.Parameter(typeof(Task));

            return (Func<Task, object>)Expression.Lambda(typeof(Func<Task, object>),
                Expression.Convert(Expression.Property(Expression.Convert(param, taskType), "Result"), typeof(object)),
                param).Compile();
        }
    }
}
