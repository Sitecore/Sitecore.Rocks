<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes" encoding="UTF-8" />

    <xsl:template match="*">
        <html>
            <head>
                <title>Query Analyzer</title>
                <style>
                    h1 {
                    font: bold 18pt verdana;
                    }
                    table {
                    border-collapse:collapse;
                    border:1px solid black;
                    margin:0px 0px 16px 0px;
                    }
                    th {
                    font: bold 9pt verdana;
                    padding:2px;
                    background:#d3dfee;
                    }
                    td {
                    font: 9pt tahoma;
                    padding:2px;
                    border:1px solid:#999999;
                    }
                </style>
            </head>
            <body>
                <h1>
                    <xsl:value-of select="/tables/@title" />
                </h1>
                <xsl:for-each select="/tables/table">
                    <table border="1">
                        <tr>
                            <xsl:for-each select="./columns/column">
                                <th>
                                    <xsl:value-of select="@name" />
                                </th>
                            </xsl:for-each>
                        </tr>
                        <xsl:for-each select="./rows/row">
                            <tr>
                                <xsl:for-each select="./value">
                                    <td>
                                        <xsl:value-of select="." />
                                    </td>
                                </xsl:for-each>
                            </tr>
                        </xsl:for-each>
                    </table>
                </xsl:for-each>
            </body>
        </html>
    </xsl:template>

</xsl:stylesheet>