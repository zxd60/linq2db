﻿using LinqToDB.Mapping;

namespace LinqToDB.DataProvider.SapHana
{
	using System.Collections.Generic;
	using Common;
	using SqlQuery;
	using SqlProvider;

	class SapHanaNativeSqlOptimizer : SapHanaSqlOptimizer
	{
		public SapHanaNativeSqlOptimizer(SqlProviderFlags sqlProviderFlags) : base(sqlProviderFlags)
		{
		}

		public override SqlStatement OptimizeStatement(SqlStatement statement, IReadOnlyParameterValues? parameterValues)
		{
			statement = base.OptimizeStatement(statement, parameterValues);

			return statement;
		}

		public override SqlStatement ConvertStatement(MappingSchema mappingSchema, SqlStatement statement, IReadOnlyParameterValues? parameterValues)
		{
			statement = base.ConvertStatement(mappingSchema, statement, parameterValues);

			// SAP HANA parameters are not just order-dependent but also name-dependent, so we cannot use
			// same parameter name
			var parameters = new HashSet<SqlParameter>(Utils.ObjectReferenceEqualityComparer<SqlParameter>.Default);

			// duplicate parameters
			statement = ConvertVisitor.ConvertAll(statement, (visitor, e) =>
			{
				if (e is ISqlExpression expr)
				{
					if (HasParameters(expr))
					{
						// prevent skipping of duplicate elements
						visitor.VisitedElements[e] = null;
					}

					if (e is SqlParameter p && !parameters.Add(p))
					{
						p = p.Clone();
						parameters.Add(p);
						return p;
					}
				}

				return e;
			});

			return statement;
		}
	}

}
