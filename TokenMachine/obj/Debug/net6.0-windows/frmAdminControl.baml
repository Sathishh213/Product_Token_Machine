   M S B A M L   `   `   `  ����G  CTokenMachine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:VendingMachineVendingMachine  5   	   $    VendingMachine.frmAdminControl   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:VendingMachine     5   	   _ [MaterialDesignThemes.Wpf, Version=4.9.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a:md1http://materialdesigninxaml.net/winfx/xaml/themes  5	   	   -   5          � Title$  frmAdminControl��5   	   $
��1080��6!   $
��1920��6/    � WindowState$ 	Maximized=�6<    � 
ResizeMode$ NoResize=�6T   $��25(�5
   	   
������� � Topmost$
 True��5   	   ��5      �� 5   
    �� ImageSource$ Images/vendingBg.jpg��6   .��5      � ���	#C8000000 � RowDefinitions 5   
   �� 5      $	��150 �6   �� 5      $��* �6   .+�5   
   ]� $��0��6   .+�5      �� $��ADMIN CONTROLS��6   $��25(�6.   $
��Bold"�6<    �� TextAlignment$ Center=�6N   $	��500��6e   $��50��6q   $	��Top=�6}    J� OpacityMask
 �   ����	#FF08090C6�   5      �� 5   
   $��1��6    �� Orientation$ Vertical=�6"   $��	10,50,0,0q�6:   .+�5      �� -   5        $��	btnRefill��5      $��Refill��6'   $��
10,10,0,10q�68   $��Center=�6d   $��Center=�6�   $��20(�6�   $
��Bold"�6�   $	��150��6�   $��50��6�      MaterialDesignRaisedButton#��[  6�   �� 5      -   5        $��btnAddProduct��5      $��Add Product��6+   $��	10,0,0,10q�6A   $��Center=�6q   $��Center=�6�   $��20(�6�   $
��Bold"�6�   $	��150��6�   $��50��6�   #��[  6�   �� 5      -   5        $��	btnReport��5      $��Report��6'   $��	10,0,0,10q�68   $��Center=�6c   $��Center=�6�   $��20(�6�   $
��Bold"�6�   $	��150��6�   $��50��6�   #��[  6�   �� 5      -   5        $��btnGotoHome��5      $��
Go to Home��6)   $��	10,0,0,10q�6>   $��Center=�6k   $��Bottom=�6�   $��20(�6�   $
��Bold"�6�   $	��150��6�   $��50��6�   #��[  6�   �� 5      -   5        $��btnShutdown��5      $��Shutdown��6)   $��	10,0,0,10q�6<   $��Center=�6i   $��Bottom=�6�   $��20(�6�   $
��Bold"�6�   $	��150��6�   $��50��6�   #��[  6�   �� 5       -   5        $��
btnRestart��5       $��Restart��6(   $��	10,0,0,10q�6:   $��Center=�6f   $��Center=�6�   $��20(�6�   $
��Bold"�6�   $	��150��6�   $��50��6�   #��[  6�   5!      5#      5$      