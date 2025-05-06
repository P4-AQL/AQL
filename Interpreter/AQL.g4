grammar AQL;

program: importStatement | baseDefinition*;
importStatement: 'import' string program;

definition: definitionComposition | baseDefinition;

definitionComposition: left = baseDefinition right = definition;

baseDefinition:
	functionDefinition
	| constDefinition
	| networks
	| simulateDefinition;

functionDefinition:
	'function' returnType = type identifier '(' formalParameterList? ')' block;

constDefinition: 'const' type assignStatement;

formalParameterList: type identifier (',' type identifier)*;

networks: queueDefinition | networkDefinition;

queueDefinition:
	'queue' identifier '{' (
		'number_of_servers:' numberOfServers = expression ','
	)? 'service:' service = expression ',' 'capacity:' capacity = expression (
		',' 'metrics:' '[' metrics? ']'
	)? '}';

networkDefinition:
	'network' identifier '{' 'inputs:' inputs = idList ';' 'outputs:' outputs = idList (
		';' 'instances:' '{' instances? '}'
	)? ';' 'routes:' '{' routesList '}' (
		';' 'metrics:' '[' metrics ']'
	)? ';'? '}';

instances: instance (';' instance)* ';'?;
instance: existing = qualifiedId ':' new = idList;

routesList: routes (',' routes)*;
routes:
	identifier '->' (routes | identifier | probabilityIdList);

probabilityIdList:
	'[' expression qualifiedId (',' expression qualifiedId)* ']';

metrics: metric (',' metric)*;
metric: namedMetric | functionMetric = functionCall;
namedMetric:
	'mrt'
	| 'vrt'
	| 'util'
	| 'throughput'
	| 'num'
	| 'avgNum';

simulateDefinition:
	'simulate' '{' 'run:' network = qualifiedId ',' 'until:' terminationCriteria = expression ','
		'times:' runs = expression '}';

statement: statementComposition | baseStatement;

statementComposition: left = baseStatement right = statement;

baseStatement:
	whileStatement
	| variableDeclarationStatement
	| assignStatement
	| ifStatement
	| returnStatement;

whileStatement:
	'while' condition = expression 'do' body = block;

variableDeclarationStatement: type assignStatement;

assignStatement:
	<assoc = right> identifier '=' expression ';';

ifStatement:
	'if' ifCondition = expression ifBody = block elseIfStatements += elseIfStatement* elseStatement?
		;
elseIfStatement: 'else if' condition = expression body = block;
elseStatement: 'else' body = block;

block: '{' statement? '}';

returnStatement: 'return' expression ';';

expressionList: expression (',' expression)*;

expression: logicalOrExpression;

logicalOrExpression:
	logicalAndExpression ('||' logicalAndExpression)*;

logicalAndExpression:
	equalityExpression ('&&' equalityExpression)*;

equalityExpression: equalExpression | inEqualExpression;

equalExpression:
	relationalExpression ('==' relationalExpression)*;
inEqualExpression:
	relationalExpression ('!=' relationalExpression)*;

relationalExpression:
	lessThanExpression
	| lessThanOrEqualExpression
	| greaterThanExpression
	| greaterThanOrEqualExpression;

lessThanExpression:
	additiveExpression ('<' additiveExpression)*;
lessThanOrEqualExpression:
	additiveExpression ('<=' additiveExpression)*;
greaterThanExpression:
	additiveExpression ('>' additiveExpression)*;
greaterThanOrEqualExpression:
	additiveExpression ('>=' additiveExpression)*;

additiveExpression: addExpression | subtractExpression;

addExpression:
	multiplicativeExpression ('+' multiplicativeExpression)*;
subtractExpression:
	multiplicativeExpression ('-' multiplicativeExpression)*;

multiplicativeExpression:
	multiplyExpression
	| divisionExpression;

multiplyExpression: unaryExpression ('*' unaryExpression)*;
divisionExpression: unaryExpression ('/' unaryExpression)*;

unaryExpression:
	negationExpression
	| negativeExpression
	| parenthesesExpression
	| value;

negationExpression: '!' expression;
negativeExpression: '-' expression;
parenthesesExpression: '(' expression ')';

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
	functionIdentifier = qualifiedId '(' parameters = expressionList? ')';

arrayInitialization: '{' value* (',' value)* '}';
arrayIndexing: target = qualifiedId '[' index = expression ']';

type: typeKeyword | arrayType;

typeKeyword:
	boolKeyword
	| intKeyword
	| doubleKeyword
	| stringKeyword;

boolKeyword: 'bool';
intKeyword: 'int';
doubleKeyword: 'double';
stringKeyword: 'string';

arrayType: '[' type ']';

qualifiedIdList: qualifiedId (',' qualifiedId)*;
qualifiedId: identifier ('.' identifier)*;

idList: identifier (',' identifier)*;
identifier: IDENTIFIER;
IDENTIFIER: [a-zA-Z][a-zA-Z0-9_]*;

bool: BOOL;
BOOL: 'true' | 'false';

int: INT;
INT: '-'? [0-9]+;

double: DOUBLE;
DOUBLE: '-'? [0-9]* '.' [0-9]+;

string: STRING;
STRING: '"' ~["\\\r\n]* '"';

WS: (' ' | '\t' | '\n' | '\r')+ -> skip;