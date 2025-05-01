grammar AQL;

program: importStatement | (definition)*;
importStatement: 'import' string program;
definition:
	constDefinition
	| functionDefinition
	| networks
	| simulateDefinition;

constDefinition: 'const' type assign;

functionDefinition:
	'function' returnType = type identifier '(' formalParameterList? ')' '{' statement? '}';

formalParameterList: type identifier (',' type identifier)?;

networks: queueDefinition | networkDefinition;

queueDefinition:
	'queue' identifier '{' (
		'number_of_servers:' numberOfServers = expression ','
	)? 'service:' service = expression ', capacity:' capacity = expression (
		',' metrics
	)? '}';

networkDefinition:
	'network' identifier '{' 'inputs:' inputs = idList ';' 'outputs:' outputs = idList (
		';' 'instances:' '{' instances = instanceList? '}'
	)? ';' 'routes:' '{' routes '}' (';' metrics)? ';'? '}';

simulateDefinition:
	'simulate' '{' 'run:' network = qualifiedId ',' 'until:' terminationCriteria = expression ','
		'times:' runs = expression '}';

instanceList: instance (';' instance)* ';'?;
instance: qualifiedId ':' idList;

routes: identifier ('->' identifier)+;

metrics: 'metrics:' '[' metricList ']';
metricList: metric (',' metric)*;
metric:
	'mrt'
	| 'vrt'
	| 'util'
	| 'throughput'
	| 'num'
	| 'avgNum';

assign: <assoc = right> identifier '=' expression ';';
statement:
	whileStatement
	| assign
	| type assign
	| ifStatement
	| returnStatement
	| statement ';' statement;

whileStatement: 'while' expression 'do' block;

ifStatement:
	'if' expression block elseIfStatement* elseStatement?;
elseIfStatement: 'else if' expression block;
elseStatement: 'else' block;
elseIf: 'else if' expression '{' statement '}' elseIf |;

block: '{' statement? '}';

returnStatement: 'return' expression ';';

expression:
	value
	| <assoc = right> ('!' | '-') expression
	| expression ('*' | '/') expression
	| expression ('+' | '-') expression
	| expression ('<' | '<=' | '>' | '>=') expression
	| expression ('==' | '!=') expression
	| expression '&&' expression
	| expression '||' expression;
value:
	functionCall
	| qualifiedId
	| string
	| double
	| int
	| bool
	| arrayInitialization
	| arrayIndexing;

functionCall:
	functionIdentifier = qualifiedId '(' parameters = qualifiedIdList? ')';

type: TYPEKEYWORD | arrayType | routeType;
arrayInitialization: '{' value* (',' value)* '}';
arrayIndexing: target = qualifiedId '[' index = expression ']';

TYPEKEYWORD: 'bool' | 'int' | 'double' | 'string';

arrayType: '[' type ']';

routeType: qualifiedId '->' qualifiedId;

qualifiedIdList: qualifiedId (',' qualifiedId)*;
qualifiedId: identifier ('.' identifier)*;

idList: identifier (',' identifier)*;
identifier: IDENTIFIER;
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;

bool: BOOL;
BOOL: 'true' | 'false';

int: INT;
INT: '-'? [0-9]+;

double: DOUBLE;
DOUBLE: '-'? [0-9]* '.' [0-9]+;

string: STRING;
STRING: '"' ~["\\\r\n]* '"';

WS: (' ' | '\t' | '\n' | '\r')+ -> skip;