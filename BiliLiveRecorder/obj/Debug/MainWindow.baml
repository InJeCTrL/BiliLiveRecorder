   M S B A M L   `   `   `  ����K  GBiliLiveRecorder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:BiliLiveRecorderBiliLiveRecorder  5   	   !    BiliLiveRecorder.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:BiliLiveRecorder     5   	   -   5          � Title$  Bilibili直播录制机��5   	   $��190.5��6   $��324.5��6.    � 
ResizeMode$ CanMinimize=�6<    � WindowStartupLocation$ CenterScreen=�6U   .��5	      � .+�5
   
   �� -   5         ��Name$ 
SearchText��5
      $��	0,34,27,0q�6&    �� TextWrapping$
 Wrap=�69   $�� ��6M   $��Right=�6U   $	��160��6q   $��24��6}   $	��Top=�6�   �� 5   
   -   5        $'��!锁定主播开播时实时录制��5      $��32,92,34,21q�6'    �� 	IsDefault . 6M   �� 5   
   -   5         ��Name$ 
SelectMode��5      $
��Left=�6'   $��	29,34,0,0q�6B   $	��Top=�6U   $��86��6m   $��24��6x    �� 
IsReadOnly .  �� SelectedIndex$ 0��6�   .@�5      �� $��主播昵称：��6   �� 5      $��主播UID：��6   5      5      5      