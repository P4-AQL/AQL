grammar AQL;

program: importList (definition)*;
importList: (importStatement)*;
importStatement: 'import' STRING;
definition:
	constDefinition
	| functionDefinition
	| networkDefinitions
	| simulateDefinition;

constDefinition: 'const' type assign;
functionDefinition:
	'function' type ID '(' formalParameterList? ')' '{' stmt '}';

networkDefinitions: queueDefinition | networkDefinition;

queueDefinition:
	'queue' ID '{' serviceCount 'service:' value ',' 'capacity:' value (
		',' metrics
	)? '}';

serviceCount: 'number_of_services:' expression ',' |;

networkDefinition:
	'network' ID '{' 'inputs:' idList ';' 'outputs:' idList (
		';' instances
	)? ';' 'routes:' '{' routes '}' (';' metrics)? ';'? '}';

simulateDefinition:
	'simulate' '{' 'run:' qualifiedID ',' 'until:' expression ',' 'times:' expression '}';

instances: 'instances:' '{' instancesList '}';
instancesList: instance (';' instance)* ';'? |;
instance: qualifiedID ':' idList;

routes:
	qualifiedID '->' qualifiedID routesB
	| qualifiedID '->' '[' routeIDList ']' routesB;
routesB:
	',' qualifiedID '->' qualifiedID routesB
	| ',' qualifiedID '->' '[' routeIDList ']' routesB
	|;
routeIDList: expression qualifiedID routeIDListB;
routeIDListB: ',' expression qualifiedID routeIDListB |;

metrics: 'metrics:' '[' metricList ']';
metricList: metric (',' metricList)* |;
metric:
	'mrt'
	| 'vrt'
	| 'util'
	| 'throughput'
	| 'num'
	| 'avgNum';

formalParameterList: type ID (',' formalParameterList)?;

assign: ID '=' expression ';';
stmt: stmtA stmt |;
stmtA:
	'while' expression 'do' stmt
	| assign
	| type assign
	| 'if' expression '{' stmt '}' else1
	| 'return' expression ';';
else1: elseIf else2 |;
else2: 'else {' stmt '}' |;
elseIf: 'else if' expression '{' stmt '}' elseIf |;
expression:
	value
	| routes
	| <assoc = right> ('!' | '-') expression
	| expression ('*' | '/') expression
	| expression ('+' | '-') expression
	| expression ('<' | '<=' | '>' | '>=') expression
	| expression ('==' | '!=') expression
	| expression '&&' expression
	| expression '||' expression;
value:
	funcCall
	| qualifiedID
	| STRING
	| DOUBLE
	| INT
	| BOOL
	| arrayInitialization
	| arrayIndexing;

actualParameterList: ID (',' ID)*;
funcCall: qualifiedID '(' actualParameterList? ')';

type: TYPEKEYWORD | arrayType | routeType;
arrayInitialization: '{' value* (',' value)* '}';
arrayIndexing: qualifiedID '[' expression ']';

TYPEKEYWORD: 'bool' | 'int' | 'double' | 'string' | 'network';

arrayType: '[' type ']';

routeType: qualifiedID '->' qualifiedID;

qualifiedIdList: qualifiedID (',' qualifiedID)*;
qualifiedID: ID ('.' ID)*;

idList: ID (',' ID)*;

ID: [a-zA-Z_][a-zA-Z0-9_]*;
BOOL: 'true' | 'false';
INT: '-'? [0-9]+;
DOUBLE: '-'? [0-9]* '.' [0-9]+;
STRING: '"' ~["\\\r\n]* '"';

WS: (' ' | '\t' | '\n' | '\r')+ -> skip;