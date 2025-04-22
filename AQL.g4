grammar AQL;

prog: importList (def)*;
importList 
  : 'import' STRING importList
  | // Epsilon
  ;
def 
  : 'const' type ID '=' expr
  | 'function' type ID '(' paramList ')' '{' stmt '}'
  | network
  ;
network
  : 'queue' ID '=' '{' serviceCount 'service:' value ',' 'capacity:' value metrics '}'
  | 'network' ID '=' '{' 'inputs:' '[' identifierList ']' ',' 'outputs:' '[' identifierList ']' ',' 'routes:' '{' routes '}' metrics '}'
  ;

identifierList
  : ID (',' ID)* 
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
  : ',' 'metrics:' '[' metricList ']'
  | //Epsilon
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
stmt
  : stmtA stmt
  | //Epsilon
  ;
stmtA 
  : 'while' expr 'do' stmt
  | ID '=' expr ';'
  | type ID '=' expr ';'
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
  : exprOr
  ;
exprOr
  : exprAnd ('||' exprAnd)*
  ;
exprAnd
  : exprEq ('&&' exprEq)*
  ;
exprEq
  : exprLess (('=='|'!=') exprLess)*
  ;
exprLess
  : exprPlus (('<'|'<='|'>'|'>=') exprPlus)*
  ;
exprPlus
  : exprTimes (('+'|'-') exprTimes)*
  ;
exprTimes
  : exprNot (('*'|'/') exprNot)*
  ;
exprNot
  : ('!'|'-')? exprFunc*
  ;
exprFunc
  : routes
  | value
  ;
value
  : funcCall
  | qualifiedID
  | STRING
  | DOUBLE
  | INT
  | BOOL
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