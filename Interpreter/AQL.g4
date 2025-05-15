grammar AQL;

programEOF: program EOF;
program: importStatement? | definition?;

importStatement: 'import' identifier program;

definition:
	functionDefinition
	| constDefinition
	| networks
	| simulateDefinition;

functionDefinition:
	'function' returnType = type identifier '(' formalParameterList? ')' block nextDefinition =
		definition?;

constDefinition:
	'const' type assignStatement nextDefinition = definition?;

formalParameterList: type identifier (',' type identifier)*;

networks: (queueDefinition | networkDefinition) nextDefinition = definition?;

queueDefinition:
	'queue' identifier '{' (
		'servers:' numberOfServers = expression ';'
	)? 'service:' service = expression ';' 'capacity:' capacity = expression (
		';' (metrics ';'?)?
	)? '}';

networkDefinition:
	'network' identifier '{' networkExpression? (
		';' networkExpression
	)* ';'? '}';

networkExpression:
	inputOutputNetworkExpression
	| instanceNetworkExpression
	| routes
	| metrics;

inputOutputNetworkExpression:
	inputs = idList '|' outputs = idList;

instanceNetworkExpression:
	existing = qualifiedId ':' new = idList;

routesList: routes (',' routes)*;
routes: routesId | routesValue;

routesId:
	qualifiedId '->' (routesId | qualifiedId | probabilityIdList);
routesValue:
	value '->' (routesId | qualifiedId | probabilityIdList);

probabilityIdList:
	'[' expression qualifiedId (',' expression qualifiedId)* ']';

metrics: '*' (metric (',' metric)*)? '*';
metric: namedMetric | functionMetric = qualifiedId;
namedMetric:
	'mrt'
	| 'vrt'
	| 'util'
	| 'throughput'
	| 'num'
	| 'avgNum';

simulateDefinition:
	'simulate' '{' 'run:' network = qualifiedId ';' 'until:' terminationCriteria = expression ';'
		'times:' runs = expression ';'? '}';

statement:
	whileStatement
	| variableDeclarationStatement
	| assignStatement
	| ifStatement
	| returnStatement;

whileStatement:
	'while' condition = expression 'do' body = block nextStatement = statement?;

variableDeclarationStatement:
	type assignStatement nextStatement = statement?;

assignStatement:
	<assoc = right> identifier '=' expression ';' nextStatement = statement?;

ifStatement:
	'if' ifCondition = expression ifBody = block elseIfStatements += elseIfStatement* elseStatement?
		nextStatement = statement?;
elseIfStatement: 'else if' condition = expression body = block;
elseStatement: 'else' body = block;

block: '{' statement? '}';

returnStatement: 'return' expression ';';

expressionList: expression (',' expression)*;

expression: expression operator expression | unaryExpression;

operator:
	andOperator
	| orOperator
	| addOperator
	| subtractOperator
	| divisionOperator
	| equalOperator
	| inEqualOperator
	| greaterThanOperator
	| greaterThanOrEqualOperator
	| lessThanOperator
	| lessThanOrEqualOperator;

andOperator: '&&';
orOperator: '||';
addOperator: '+';
subtractOperator: '-';
divisionOperator: '/';
equalOperator: '==';
inEqualOperator: '!=';
greaterThanOperator: '>';
greaterThanOrEqualOperator: '>=';
lessThanOperator: '<';
lessThanOrEqualOperator: '<=';

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
	| anyIdentifier
	| string
	| double
	| int
	| bool
	| arrayInitialization
	| arrayIndexing;

functionCall:
	functionIdentifier = anyIdentifier '(' parameters = expressionList? ')';

arrayInitialization: '{' expression* (',' expression)* '}';
arrayIndexing: target = identifier '[' index = expression ']';

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

anyIdentifier: identifier | qualifiedId;

qualifiedIdList: qualifiedId (',' qualifiedId)*;
qualifiedId: left = identifier '.' right = identifier;

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

WS: (WHITESPACE | TABS | NEWLINES)+ -> skip;
WHITESPACE: ' ';
TABS: '\t';
NEWLINES: '\n' | '\r';

COMMENTS: (ONE_LINE_COMMENT | MULTI_LINE_COMMENT)+ -> skip;
ONE_LINE_COMMENT: '//' ~[\r\n];
MULTI_LINE_COMMENT: '/*' .*? '*/';