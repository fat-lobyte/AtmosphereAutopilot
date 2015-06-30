function [y1] = myNeuralNetworkFunction(x1)
%MYNEURALNETWORKFUNCTION neural network simulation function.
%
% Generated by Neural Network Toolbox function genFunction, 01-Jul-2015 02:15:49.
% 
% [y1] = myNeuralNetworkFunction(x1) takes these arguments:
%   x = 2xQ matrix, input #1
% and returns:
%   y = 1xQ matrix, output #1
% where Q is the number of samples.

%#ok<*RPMT0>

  % ===== NEURAL NETWORK CONSTANTS =====
  
  % Input 1
  x1_step1_xoffset = [-0.13948168;-0.91200006];
  x1_step1_gain = [6.91671490210722;1.05178265748602];
  x1_step1_ymin = -1;
  
  % Layer 1
  b1 = [-1.702654968332419;-1.5054493944710736;2.2051630134390448;-0.32579732557313423;-1.4718850312623091;1.694011106425148];
  IW1_1 = [2.5210715530953718 0.3077595549614478;2.3531773622871439 0.20070138149572966;-0.22279290332248605 -6.8626796299000707;-1.124369830093406 0.69637040911885706;-3.5814224834026915 1.4225202749596217;4.0602544626295938 -1.5567170909504142];
  
  % Layer 2
  b2 = 1.6758988788823777;
  LW2_1 = [-8.2258037310712986 10.02351312998233 0.18766305741134021 8.4909339326508277 -8.835358199003128 -6.3056976153765261];
  
  % Output 1
  y1_step1_ymin = -1;
  y1_step1_gain = 0.484347229497521;
  y1_step1_xoffset = -1.7409996;
  
  % ===== SIMULATION ========
  
  % Dimensions
  Q = size(x1,2); % samples
  
  % Input 1
  xp1 = mapminmax_apply(x1,x1_step1_gain,x1_step1_xoffset,x1_step1_ymin);
  
  % Layer 1
  a1 = tansig_apply(repmat(b1,1,Q) + IW1_1*xp1);
  
  % Layer 2
  a2 = repmat(b2,1,Q) + LW2_1*a1;
  
  % Output 1
  y1 = mapminmax_reverse(a2,y1_step1_gain,y1_step1_xoffset,y1_step1_ymin);
end

% ===== MODULE FUNCTIONS ========

% Map Minimum and Maximum Input Processing Function
function y = mapminmax_apply(x,settings_gain,settings_xoffset,settings_ymin)
  y = bsxfun(@minus,x,settings_xoffset);
  y = bsxfun(@times,y,settings_gain);
  y = bsxfun(@plus,y,settings_ymin);
end

% Sigmoid Symmetric Transfer Function
function a = tansig_apply(n)
  a = 2 ./ (1 + exp(-2*n)) - 1;
end

% Map Minimum and Maximum Output Reverse-Processing Function
function x = mapminmax_reverse(y,settings_gain,settings_xoffset,settings_ymin)
  x = bsxfun(@minus,y,settings_ymin);
  x = bsxfun(@rdivide,x,settings_gain);
  x = bsxfun(@plus,x,settings_xoffset);
end
