   M S B A M L   `   `   `  ����G  CTokenMachine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:VendingMachineVendingMachine  5   	   ?$clr-namespace:VendingMachine.HelpersVendingMachine.Helpers  5      "    VendingMachine.frmHomeScreen   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:VendingMachine     5   	   ;
localmodel$clr-namespace:VendingMachine.Helpers     5      -   5          � Title$  frmHomeScreen��5	   	   $
��1080��5
   	   $
��1920��6    � WindowState$ 	Maximized=�6%    � 
ResizeMode$ NoResize=�6=   
�������
���   �$��30(�5   /   $
��Bold"�6=    � Topmost$
 True��6O    � 	Resources 5         roundButtonTemplate �� 
TargetType �� Property �� Value �� 
TargetType	 A�Name
 �� Triggers P� Property P� Value �� 
TargetName  
btnAnimate g� RepeatBehavior �� TargetProperty �� 
TargetName9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value��%�  &	        &	 �    �� 5   
   " ��6-   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      A� $	 	OuterRing��6&   #��z��6:   �� 5      $��Center=�6/   $��Center=�6L   5      
 5      P� 5       � ��$
 True��6<   . �5   "   �� $ 	OuterRing��6)    � ��$	 210��6T   �� 5   "   $ 	OuterRing��6)    � ��$	 210��6S   5      5      5      5       �� 5!   
   $ Forever��6*   .��5"      U� $ (FrameworkElement.Width)�6,   $ btnFoodProduct��6a   .\�5#        $ 0:0:0��6'   $	 420Q�67     5$      $ 0:0:0.4��6'   $	 400Q�69     5%      $ 0:0:0.8��6'   $	 420Q�69   5&      U� 5'      $ (FrameworkElement.Height)�6,   $ btnFoodProduct��6b   .\�5(        $ 0:0:0��6'   $	 165Q�67     5)      $ 0:0:0.4��6'   $	 145Q�69     5*      $ 0:0:0.8��6'   $	 165Q�69   5+      5,      5-       � Triggers 5.      :� 5/   
    :� RoutedEvent$ FrameworkElement.Loaded��6   .[�50      �� #��[ 6   51      .��53      �  � RowDefinitions 54   
   �� 55      �� 56      $��60 �6   .+�58   
   � $��0��6    � ColumnDefinitions 59      �� 5:      $	��700 �6#   �� 5;      5=      � 5>   
   $��0��6   $��0��6   .+�5?      �� -   5         ��Name$ imageControl��5?      $
��Fill=�6+   $��
20,0,867,0q�6:   �� 5@      $��Images/Logo.png��6   $	��100��6.   $	��Top=�6;   $
��Left=�6S   $��
20,10,20,0q�6n   5A      � 5D   
   $��0��6   $��1��6   .+�5E      r� -   5         r�Name$ meVideo��5E       r� Volume$ 1Q�6-    r� Stretch$
 Fill=�68   $��	590,0,0,0q�6�   �� 5F      -   5        $��btnFoodProduct��5F   0   
������ $	��100��6d   $	��300��6q   $��Bottom=�6~   $��Right=�6�   $��	0,0,70,10q�6�   ��5G      �� 5H       �� ImageSource$ Images/button.png��6!   .��5J      �� $��Center=�6   $��Center=�6;   $	��260��6V    J� RenderTransformOrigin$ 0.636,0.648&�6b   .+�5K      �� $��90,30,10,10q�6    $��
ORDER HERE��65   
���   �$
��Bold"�6\   $��Center=�6n   $��24(�6�   $ 1.258,1.772&�6�   $	��150��6�   5M      5N      5O      �� 5T   
   -   5         ��Name$ stack��5T   ;   $��1��6    �� Orientation$ 
Horizontal=�6"    5U      .   (VendingMachine.Helpers.NegatingConverter  NegatingConverter 	 slide g� Duration  W� From! �� ElementName	" W� To# �� 	Converter��%
  &	       &	      7    5V      6/   �� 5W      .��5X      W� $ 00:00:20D�6�   $ X�5Y   '   $ transferCurreny��5Z   '   $ Forever��5[   '     5X   &   �� *Width+$! canvas��" 6Q   �� *Width+$! canvas��8#   5[   '   5\      5]      z�5^      T� 5_      -   5        transferCurreny��$ T� X$$ 0Q�5_   >    5a      :� 5b      $ StackPanel.Loaded��6   .[�5c      �� #��[ 6&   5d      :� 5e      $ StackPanel.SizeChanged��6   .[�5f      �� #��[ 6&   5g      .+�5i      �� -   5        % ��Name$% canvas��5i      ��6%   �� *ActualWidth+$! stack��.+�5j      �� -   5        & ��Name$& txtKron��5j   F   $�� ��6   $��25(�6$   
�������' �� Left$' 0��6W   �� 5k      $��25(�6G   ��6   �� *Text+$! txtKron��' 6U   �� *Width+$! canvas��## [ 5l      5m      ]� 5r   
   $��2��6   .+�5s      �� -	   5        $��txtHelpLine��5s      $��50��6,   $��Help Line : ��68   ���	#7F000000$��30(�6c   $��Bottom=�6q   ( �� TextAlignment$( Center=�6�   5t      5w      5x      