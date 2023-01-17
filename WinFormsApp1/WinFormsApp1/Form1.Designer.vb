<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.LabelHitCount = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'LabelHitCount
        '
        Me.LabelHitCount.AutoSize = True
        Me.LabelHitCount.Location = New System.Drawing.Point(0, 0)
        Me.LabelHitCount.Name = "LabelHitCount"
        Me.LabelHitCount.Size = New System.Drawing.Size(35, 15)
        Me.LabelHitCount.TabIndex = 0
        Me.LabelHitCount.Text = "らべる"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(184, 161)
        Me.Controls.Add(Me.LabelHitCount)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents LabelHitCount As Label
End Class
