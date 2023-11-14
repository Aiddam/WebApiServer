using Server.Interfaces;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Server.Extractors
{
    public class TaskExtractor : IExtractor<Task>
    {
        private readonly ConcurrentDictionary<Type, Func<Task, object>> _extractors = new();

        public object? ExtractValue(Task input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            Type taskType = input.GetType();
            if (!taskType.IsGenericType || !input.IsCompleted)
            {
                return null;
            }

            var extractor = _extractors.GetOrAdd(taskType, CreateExtractor);

            return extractor(input);
        }

        private Func<Task, object> CreateExtractor(Type taskType)
        {
            if (!typeof(Task).IsAssignableFrom(taskType))
            {
                throw new InvalidOperationException("Invalid task type.");
            }
            ParameterExpression param = Expression.Parameter(typeof(Task), "task");
            UnaryExpression taskCast = Expression.Convert(param, taskType);
            MemberExpression propertyAccess = Expression.Property(taskCast, "Result");
            UnaryExpression convertToObj = Expression.Convert(propertyAccess, typeof(object));

            return Expression.Lambda<Func<Task, object>>(convertToObj, param).Compile();
        }
    }
}
