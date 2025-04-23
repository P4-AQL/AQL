grammar AQL;

prog: importList (def)*;
importList 
  : 'import' STRING importList
  | // Epsilon
  ;
def 
  : 'const' type assign
  | 'function' type ID '(' paramList ')' '{' stmt '}'
  | network
  | 'simulate' '{' 'run:' qualifiedID ',' 'until:' expr ',' 'times:' expr '}'
  ;
network
  : 'queue' ID '{' serviceCount 'service:' value ',' 'capacity:' value (',' metrics)? '}'
  | 'network' ID '{' 'inputs:' idList ';' 'outputs:' idList (';' instances)? ';' 'routes:' '{' routes '}' (';' metrics)? ';'? '}'
  ;

idList
  : ID (',' ID)* 
  ;
qualifiedIdList
  : qualifiedID (',' qualifiedID)*
  ;
instances
  : 'instances:' '{' instancesList '}'
  ;
instancesList
  : instance (';' instance)* ';'?
  | //Epsilon
  ;
instance
  : qualifiedID ':' idList
  ;

routes
  : qualifiedID '->' qualifiedID routesB
  | qualifiedID '->' '[' routeIDList ']' routesB
  ;
routesB
  : ',' qualifiedID '->' qualifiedID routesB
  | ',' qualifiedID '->' '[' routeIDList ']' routesB
  | //Epsilon
  ;
routeIDList
  : expr qualifiedID routeIDListB
  ;
routeIDListB
  : ',' expr qualifiedID routeIDListB
  | //Epsilon
  ;

metrics
  : 'metrics:' '[' metricList ']'
  ;
metricList
  : metric metricListA
  | //Epsilon
  ;
metricListA
  : ',' metric metricListA
  | //Epsilon
  ;
metric
  : 'mrt'
  | 'vrt'
  | 'util'
  | 'throughput'
  | 'num'
  | 'avgNum'
  ;

serviceCount
  : 'number_of_services:' expr ','
  | //Epsilon
  ;
paramList 
  : type qualifiedID paramListA
  | //Epsilon
  ;
paramListA 
  : ',' type qualifiedID paramListA
  | //Epsilon
  ;
assign
  : ID '=' expr ';'
  ;
stmt
  : stmtA stmt
  | //Epsilon
  ;
stmtA 
  : 'while' expr 'do' stmt
  | assign
  | type assign
  | 'if' expr '{' stmt '}' else1
  | 'return' expr ';'
  ;
else1
  : elseIf else2
  | //Epsion
  ;
else2
  : 'else {' stmt '}'
  | //Epsilon
  ;
elseIf
  : 'else if' expr '{' stmt '}' elseIf
  | //Epsilon
  ;
expr 
  : value
  | routes
  | <assoc=right> ('!'|'-') expr
  | expr ('*'|'/') expr
  | expr ('+'|'-') expr
  | expr ('<'|'<='|'>'|'>=') expr
  | expr ('=='|'!=') expr
  | expr '&&' expr
  | expr '||' expr
  ;
value
  : funcCall
  | qualifiedID
  | STRING
  | DOUBLE
  | INT
  | BOOL
  | array
  | arrayValue
  ;

actualParamList 
  : qualifiedID paramListA
  | value paramListA
  | //Epsilon
  ;
actualParamListA 
  : ',' qualifiedID paramListA
  | ',' value paramListA
  | //Epsilon
  ;
funcCall
  : qualifiedID '(' actualParamList ')'
  ;

type
  : 'bool'
  | 'int'
  | 'double'
  | 'string'
  | 'network'
  | '[' type ']'
  | '(' type '->' type ')'
  ;
array
  : '{' value* (',' value)* '}'
  ;
arrayValue
  : qualifiedID'['expr']'
  ;
ID
  : [a-zA-Z_][a-zA-Z0-9_]*
  ;
qualifiedID
  : ID ('.' ID)*
  ;
BOOL
  : 'true'
  | 'false'
  ;
INT
  : '-'? [0-9]+
  ;
DOUBLE
  : '-'? [0-9]* '.' [0-9]+
  ;
STRING
  :  '"' ~["\\\r\n]* '"'
  ;

WS
  : (' ' | '\t' | '\n')+ -> skip
  ;